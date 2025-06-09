package com.example.nostore

import android.content.Context
import coil.intercept.Interceptor
import okhttp3.OkHttpClient
import okhttp3.Response
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.security.cert.X509Certificate
import javax.net.ssl.SSLContext
import javax.net.ssl.TrustManager
import javax.net.ssl.X509TrustManager

class AuthInterceptor(private val context: Context) : okhttp3.Interceptor {
    private val tokenManager = TokenManager
    override fun intercept(chain: okhttp3.Interceptor.Chain): Response {
        val request = chain.request()
        val token = tokenManager.getAuthToken(context)

        val authenticatedRequest = if (token != null) {
            request.newBuilder()
                .header("Authorization", "Bearer $token")
                .build()
        } else {
            request
        }

        return chain.proceed(authenticatedRequest)
    }
}

fun createUnsafeOkHttpClient(): OkHttpClient {
    try {
        val trustAllCerts = arrayOf<TrustManager>(
            object : X509TrustManager {
                override fun getAcceptedIssuers(): Array<X509Certificate?> = arrayOf()

                override fun checkClientTrusted(certs: Array<X509Certificate>, authType: String) {}

                override fun checkServerTrusted(certs: Array<X509Certificate>, authType: String) {}
            }
        )

        val sslContext = SSLContext.getInstance("SSL")
        sslContext.init(null, trustAllCerts, null)

        return OkHttpClient.Builder()
            .sslSocketFactory(sslContext.socketFactory, trustAllCerts[0] as X509TrustManager)
            .hostnameVerifier { _, _ -> true } // Игнорируем проверку хоста
            .build()
    } catch (e: Exception) {
        throw RuntimeException(e)
    }
}

object NetworkClient {
    private lateinit var appContext: Context

    fun initialize(context: Context) {
        appContext = context.applicationContext
    }
    val authApi: ApiService by lazy {
        val client = OkHttpClient.Builder()
            .addInterceptor(AuthInterceptor(appContext))
            .build()

        Retrofit.Builder()
            .baseUrl("http://10.0.2.2:7168/")
            .client(client)
            .addConverterFactory(GsonConverterFactory.create())
            .build()
            .create(ApiService::class.java)
    }
}
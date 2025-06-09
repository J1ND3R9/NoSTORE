package com.example.nostore

import android.net.Network
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

class AuthRepository {

    private val api = NetworkClient.authApi

    suspend fun login(email: String, password: String): Result<LoginResponse> =
        withContext(Dispatchers.IO) {
            try {
                val response = api.login(LoginRequest(email, password))
                if (response.isSuccessful && response.body() != null) {
                    Result.success(response.body()!!)
                } else {
                    Result.failure(Exception("Мы не нашли такого пользователя! Возможно, вы ошиблись в данных?"))
                }
            } catch (e: Exception) {
                Result.failure(e)
            }
        }

    suspend fun fetchUserInfo(): Result<UserInfo> =
        withContext(Dispatchers.IO) {
            try {
                val response = api.getUserInfo()
                if (response.isSuccessful && response.body() != null) {
                    Result.success(response.body()!!)
                } else {
                    Result.failure(Exception("Ошибка загрузки данных пользователя"))
                }
            } catch (e: Exception) {
                Result.failure(e)
            }
        }

    suspend fun getProductStatus(productId: String): Result<ProductStatus> =
        withContext(Dispatchers.IO) {
            try {
                val response = api.getProductStatus(productId)
                if (response.isSuccessful && response.body() != null) {
                    Result.success(response.body()!!)
                } else {
                    Result.failure(Exception("Ошибка загрузки статуса продукта"))
                }
            } catch (e: Exception) {
                Result.failure(e)
            }
        }
}
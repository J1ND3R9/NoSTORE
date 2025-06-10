package com.example.nostore

import ProductDto
import android.R
import android.content.Context
import android.os.Environment
import androidx.compose.ui.platform.LocalContext
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.nostore.NetworkClient.authApi
import com.google.gson.annotations.SerializedName
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import okhttp3.ResponseBody
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.Path
import retrofit2.http.Streaming
import java.io.File
import java.io.FileOutputStream

interface ApiService {
    @GET("/api/apiproduct/all")
    suspend fun getAllProducts(): List<ProductDto>

    @POST("/api/auth/login")
    suspend fun login(@Body loginRequest: LoginRequest): Response<LoginResponse>

    @GET("/api/auth/me")
    suspend fun getUserInfo(): Response<UserInfo>

    @GET("/api/apiproduct/{productId}")
    suspend fun getProductStatus(@Path("productId") productId: String): Response<ProductStatus>

    @POST("/api/apiproduct/add_product_cart")
    suspend fun addToCart(@Body request: ProductRequest): Response<CartApiDto>

    @POST("/api/apiproduct/remove_product_cart")
    suspend fun removeFromCart(@Body request: ProductRequest): Response<CartApiDto>

    @POST("/api/apiproduct/quantity_product_cart")
    suspend fun changeQuantity(@Body request: ProductRequest): Response<CartApiDto>

    @POST("/api/apiproduct/select_product_cart")
    suspend fun selectProduct(@Body request: ProductRequest): Response<CartApiDto>

    @POST("/api/apiproduct/unselect_product_cart")
    suspend fun unselectProduct(@Body request: ProductRequest): Response<CartApiDto>

    @GET("/api/apiproduct/getCart")
    suspend fun getCart(): Response<CartApiDto>

    @GET("/api/apiproduct/getFavorite")
    suspend fun getFavorites(): Response<List<ProductDto>>

    @GET("/api/apiproduct/get/{productId}")
    suspend fun getProductById(@Path("productId") productId: String): Response<ProductDto>

    @POST("/api/apiproduct/add_product_favorite")
    suspend fun addFavoriteProduct(@Body request: ProductRequest): Response<Unit>

    @POST("/api/apiproduct/remove_product_favorite")
    suspend fun removeFavoriteProduct(@Body request: ProductRequest): Response<Unit>

    @POST("/api/apiproduct/add_compare")
    suspend fun addCompareProduct(@Body request: ProductRequest): Response<Unit>

    @POST("/api/apiproduct/remove_compare")
    suspend fun removeCompareProduct(@Body request: ProductRequest): Response<Unit>

    @GET("/api/catalog")
    suspend fun getCategories(): Response<List<CategoryDto>>

    @GET("/api/catalog/{slug}")
    suspend fun getCategory(@Path("slug") slug: String): Response<CategoryResponse>

    @POST("/catalog/getfilteredproducts")
    suspend fun getFilteredProducts(@Body request: FilterRequest): Response<List<ProductDto>>

    @GET("/search/{query}")
    suspend fun search(@Path("query") query: String): List<ProductDto>

    @POST("/api/order/place")
    suspend fun placeOrder(): Response<OrderResponse>

    @Streaming
    @GET("/receipts/download/{orderId}")
    suspend fun downloadReceipt(@Path("orderId") orderId: String): Response<ResponseBody>
}

data class OrderResponse(val orderid: String)

class OrderViewModel : ViewModel() {

    private val _placeOrderState = MutableStateFlow<Result<String>?>(null)
    val placeOrderState: StateFlow<Result<String>?> = _placeOrderState

    fun placeOrder(api: ApiService) {
        viewModelScope.launch {
            try {
                val response = api.placeOrder()
                if (response.isSuccessful && response.body() != null) {
                    _placeOrderState.value = Result.success(response.body()!!.orderid)
                } else {
                    _placeOrderState.value = Result.failure(Exception("Ошибка оформления заказа"))
                }
            } catch (e: Exception) {
                _placeOrderState.value = Result.failure(e)
            }
        }
    }
}

class ReceiptDownloader(private val context: Context) {

    suspend fun downloadReceipt(orderId: String, api: ApiService): Result<String> {
        return withContext(Dispatchers.IO) {
            try {
                val response = api.downloadReceipt(orderId)
                if (!response.isSuccessful || response.body() == null) {
                    return@withContext Result.failure(Exception("Ошибка загрузки чека"))
                }

                // Открываем поток
                val inputStream = response.body()!!.byteStream()

                // Путь: например, папка "Downloads"
                val downloadsDir = context.getExternalFilesDir(Environment.DIRECTORY_DOWNLOADS)
                val outputFile = File(downloadsDir, "$orderId.pdf")

                // Сохраняем
                FileOutputStream(outputFile).use { output ->
                    val buffer = ByteArray(4 * 1024)
                    var bytesRead: Int
                    while (inputStream.read(buffer).also { bytesRead = it } != -1) {
                        output.write(buffer, 0, bytesRead)
                    }
                }

                Result.success(outputFile.absolutePath)
            } catch (e: Exception) {
                Result.failure(e)
            }
        }
    }
}


data class FilterRequest(
    val category: String,
    val dictionary: Map<String, Map<String, List<String>>>,
    val minprice: Int?,
    val maxprice: Int?,
    val sort: String?
)

data class LoginRequest(val email: String, val password: String)
data class LoginResponse(
    val token: String,
    val userId: String
)

data class UserInfo(
    val isAuthenticated: Boolean,
    val userId: String?,
    val nickname: String?,
    val avatar: String?,
    val role: String?
)

data class ProductRequest(
    val ProductId: String? = null,
    val ProductIds: List<String>? = null,
    val Quantity: Int? = null
)

data class OrderDto(
    val Id: String,
    val CreateDate: String,
    val TotalPrice: Int,
    val Status: String
)

data class CartApiDto(
    @SerializedName("Items") val items: List<CartItemApiDto> = emptyList(),
    @SerializedName("TotalCost") val totalCost: Int = 0,
    @SerializedName("SelectedCount") val selectedCount: Int = 0
)

data class CartItemApiDto(
    @SerializedName("Product") val productDto: ProductDto,
    @SerializedName("Quantity") val quantity: Int = 0,
    @SerializedName("IsSelected") val isSelected: Boolean = false
) {
    val totalPrice: Int
        get() = productDto.Price * quantity
}

data class CategoryDto(
    val _id: String,
    val Name: String,
    val Slug: String,
    val Image: String,
    val HasSubcategories: Boolean,
    val Subcategories: List<CategoryDto>? = null
)

data class CategoryResponse(
    val IsSubcategory: Boolean,
    val Name: String,
    val Subcategories: List<CategoryDto>? = null,
    val Products: List<ProductDto>? = null,
    val CategoryName: String? = null,
    val Filter: FilterDto? = null
)

data class FilterDto(
    val category: String,
    val properties: Map<String, Map<String, List<String>>>
)
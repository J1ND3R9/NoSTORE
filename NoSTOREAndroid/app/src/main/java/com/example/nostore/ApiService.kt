package com.example.nostore

import ProductDto
import com.google.gson.annotations.SerializedName
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.Path

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
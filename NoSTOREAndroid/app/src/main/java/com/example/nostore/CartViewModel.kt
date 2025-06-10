import android.content.Context
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.nostore.CartItemApiDto
import com.example.nostore.ProductRequest
import com.example.nostore.ProductStatus
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch
import kotlin.onSuccess

class CartViewModel : ViewModel() {

    private val _uiState = MutableStateFlow<CartUiState>(CartUiState.Loading)
    val uiState: StateFlow<CartUiState> = _uiState

    private val repository = CartRepository()

    fun getProductStatus(productId: String): ProductStatus? {
        var result: ProductStatus? = null
        viewModelScope.launch {
            val response = repository.getProductStatus(productId)
            result = response.getOrNull()
        }
        return result
    }

    private var currentProduct by mutableStateOf<ProductDto?>(null)
    public var isFavorite by mutableStateOf(false)
    public var isInCompare by mutableStateOf(false)

    fun setProduct(product: ProductDto) {
        currentProduct = product
        isFavorite = product.isFavorite
        isInCompare = product.inCompare
    }

    fun loadCart() {
        viewModelScope.launch {
            _uiState.value = CartUiState.Loading

            val result = repository.getCartWithStatus()
            result.onSuccess { items ->
                _uiState.value = CartUiState.Success(items)
            }.onFailure {
                _uiState.value = CartUiState.Error(it.message ?: "Ошибка загрузки")
            }
        }
    }

    fun loadFavs() {
        viewModelScope.launch {
            _uiState.value = CartUiState.Loading

            val result = repository.getFavoriteWithStatus()
            result.onSuccess { items ->
                _uiState.value = CartUiState.SuccessListProducts(items)
            }.onFailure {
                _uiState.value = CartUiState.Error(it.message ?: "Ошибка загрузки")
            }
        }
    }
    fun addToFavorite(productId: String) {
        viewModelScope.launch {
            repository.addFavorite(productId = productId)
            loadFavs()
        }
    }

    fun removeFromFavorite(productId: String) {
        viewModelScope.launch {
            repository.removeFavorite(productId = productId)
            loadFavs()
        }
    }

    fun addToCart(productId: String) {
        viewModelScope.launch {
            val result = repository.addToCart(ProductRequest(ProductId = productId))
            result.onSuccess { cart ->
                _uiState.value = CartUiState.Success(cart.items)
            }
        }
    }

    fun removeFromCart(productId: String) {
        viewModelScope.launch {
            repository.removeFromCart(ProductRequest(ProductId = productId))
            loadCart()
        }
    }

    fun changeQuantity(productId: String, quantity: Int) {
        viewModelScope.launch {
            val result = repository.changeQuantity(ProductRequest(ProductId = productId, Quantity = quantity))
            result.onSuccess { cart ->
                _uiState.value = CartUiState.Success(cart.items)
            }
        }
    }

    fun updateSelection(productId: String, isSelected: Boolean) {
        viewModelScope.launch {
            if (isSelected) {
                repository.selectProduct(ProductRequest(ProductId = productId))
            } else {
                repository.unselectProduct(ProductRequest(ProductId = productId))
            }
        }
    }
}

sealed class CartUiState {
    object Loading : CartUiState()
    data class Success(val items: List<CartItemApiDto>) : CartUiState()
    data class Error(val message: String) : CartUiState()
    data class SuccessListProducts(val items: List<ProductDto>) : CartUiState()
}
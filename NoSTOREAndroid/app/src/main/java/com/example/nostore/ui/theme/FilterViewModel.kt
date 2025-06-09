import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.nostore.FilterRequest
import com.example.nostore.ProductRepository
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

data class FilterUiState(
    val products: List<ProductDto> = emptyList(),
    val filters: Map<String, Map<String, List<String>>> = emptyMap(),
    val minPrice: String = "",
    val maxPrice: String = "",
    val sortType: String = "popular"
)

class FilterViewModel(private val repository: ProductRepository) : ViewModel() {

    private val _state = MutableStateFlow(FilterUiState())
    val state: StateFlow<FilterUiState> = _state

    fun updateProducts(products: List<ProductDto>) {
        _state.value = _state.value.copy(products = products)
    }

    fun applyFilters(category: String) {
        viewModelScope.launch {
            val request = FilterRequest(
                category = category,
                dictionary = _state.value.filters,
                minprice = _state.value.minPrice.toIntOrNull(),
                maxprice = _state.value.maxPrice.toIntOrNull(),
                sort = _state.value.sortType
            )

            val result = repository.getFilteredProducts(request)
            result.onSuccess { filteredProducts ->
                _state.value = _state.value.copy(products = filteredProducts)
            }.onFailure {
                // TODO: обработка ошибки
            }
        }
    }

    fun updateMinPrice(value: String) {
        _state.value = _state.value.copy(minPrice = value)
    }

    fun updateMaxPrice(value: String) {
        _state.value = _state.value.copy(maxPrice = value)
    }

    fun updateFilters(filters: Map<String, Map<String, List<String>>>) {
        _state.value = _state.value.copy(filters = filters)
    }
}
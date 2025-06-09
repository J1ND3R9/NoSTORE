package com.example.nostore

import CartRepository
import ProductDto
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

class ProductViewModel : ViewModel() {

    private val _uiState = MutableStateFlow<ProductUiState>(ProductUiState.Loading)

    val uiState: StateFlow<ProductUiState> = _uiState

    private val repository = ProductRepository()

    fun loadProduct(productId: String) {
        viewModelScope.launch {
            _uiState.value = ProductUiState.Loading

            val result = repository.getFullProductById(productId)

            result.onSuccess { product ->
                _uiState.value = ProductUiState.Success(product)
            }.onFailure {
                _uiState.value = ProductUiState.Error(it.message ?: "Ошибка")
            }
        }
    }
}

sealed class ProductUiState {
    object Loading : ProductUiState()
    data class Success(val status: ProductDto) : ProductUiState()
    data class Error(val message: String) : ProductUiState()
}

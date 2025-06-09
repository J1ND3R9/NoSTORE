package com.example.nostore

import ProductDto
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

sealed class CategoryUiState {
    object Loading : CategoryUiState()
    data class Categories(val items: List<CategoryDto>) : CategoryUiState()
    data class Subcategories(val name: String, val subcategories: List<CategoryDto>) : CategoryUiState()
    data class ShowProducts(val categoryName: String, val products: List<ProductDto>, val filters: FilterDto?) : CategoryUiState()
    data class Error(val message: String) : CategoryUiState()
}

class CategoryViewModel : ViewModel() {

    private val _uiState = MutableStateFlow<CategoryUiState>(CategoryUiState.Loading)
    val uiState: StateFlow<CategoryUiState> = _uiState

    private val repository = CategoryRepository()

    // Загружаем корневые категории
    fun loadRootCategories() {
        viewModelScope.launch {
            _uiState.value = CategoryUiState.Loading
            val result = repository.getRootCategories()
            result.onSuccess { categories ->
                _uiState.value = CategoryUiState.Categories(categories)
            }.onFailure {
                _uiState.value = CategoryUiState.Error(it.message ?: "Ошибка")
            }
        }
    }

    // Переход по slug
    fun navigateTo(slug: String) {
        viewModelScope.launch {
            _uiState.value = CategoryUiState.Loading
            val result = repository.getCategory(slug)
            result.onSuccess { response ->
                if (response.IsSubcategory) {
                    _uiState.value = CategoryUiState.Subcategories(
                        name = response.Name,
                        subcategories = response.Subcategories ?: emptyList()
                    )
                } else {
                    _uiState.value = CategoryUiState.ShowProducts(
                        categoryName = response.CategoryName ?: "Неизвестная категория",
                        products = response.Products ?: emptyList(),
                        filters = response.Filter
                    )
                }
            }.onFailure {
                _uiState.value = CategoryUiState.Error(it.message ?: "Ошибка")
            }
        }
    }
}
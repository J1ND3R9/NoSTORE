package com.example.nostore

import android.content.Context
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

class LoginViewModel : ViewModel() {
    private val _uiState = MutableStateFlow<LoginUiState>(LoginUiState.Initial)
    val uiState: StateFlow<LoginUiState> = _uiState

    private val repository = AuthRepository()

    fun login(email: String, password: String, context: Context) {
        viewModelScope.launch {
            _uiState.value = LoginUiState.Loading

            // Шаг 1: Логин
            val loginResult = repository.login(email, password)
            loginResult.onSuccess { loginResponse ->
                TokenManager.saveToken(context, loginResponse.token, loginResponse.userId)

                // Шаг 2: Получаем данные пользователя
                val userInfoResult = repository.fetchUserInfo()
                userInfoResult.onSuccess { userInfo ->
                    TokenManager.saveUserInfo(context, userInfo)
                    _uiState.value = LoginUiState.Success
                }.onFailure {
                    _uiState.value = LoginUiState.Error(it.message ?: "Не удалось загрузить данные")
                }

            }.onFailure {
                _uiState.value = LoginUiState.Error(it.message ?: "Ошибка входа")
            }
        }
    }
}

sealed class LoginUiState {
    object Initial : LoginUiState()
    object Loading : LoginUiState()
    object Success : LoginUiState()
    data class Error(val message: String) : LoginUiState()
}
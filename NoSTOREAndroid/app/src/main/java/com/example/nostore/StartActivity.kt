package com.example.nostore

import FavoriteScreen
import ProductDetailsScreen
import android.content.Context
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.compose.animation.AnimatedVisibility
import androidx.compose.animation.fadeIn
import androidx.compose.animation.fadeOut
import androidx.compose.animation.scaleIn
import androidx.compose.animation.scaleOut
import androidx.compose.animation.slideInVertically
import androidx.compose.animation.slideOutVertically
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.text.BasicTextField
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Search
import androidx.compose.material3.Button
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.Icon
import androidx.compose.material3.LocalTextStyle
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.OutlinedTextFieldDefaults
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.derivedStateOf
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.SolidColor
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.navigation.NavType
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import androidx.navigation.navArgument
import com.example.nostore.ui.theme.NoSTORETheme

class StartActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        NetworkClient.initialize(this)
        enableEdgeToEdge()
        setContent {
            NoSTORETheme {
                Scaffold(modifier = Modifier.fillMaxSize()) { innerPadding ->
                    val context = LocalContext.current
                    val navController = rememberNavController()
                    val startDestination = if (TokenManager.isLoggedIn(context)) {
                        "home"
                    } else {
                        "login"
                    }
                    NavHost(
                        navController = navController,
                        startDestination = startDestination,
                        modifier = Modifier.padding(innerPadding)
                    ) {
                        composable("login") {
                            LoginScreen(context = this@StartActivity) {
                                navController.navigate("home") {
                                    popUpTo("login") {
                                        inclusive = true
                                    }
                                }
                            }
                        }
                        composable("home") {
                            MainLayout (
                                content = { padding ->
                                    Column (
                                        modifier = Modifier
                                            .fillMaxSize()
                                            .padding(padding)
                                            .padding(16.dp)
                                    ) {
                                        RecentlyProducts(navController = navController)
                                    }
                                },
                                navController = navController
                            )
                        }
                        composable("favorite") {
                            MainLayout (
                                content = { padding ->
                                    Column (
                                        modifier = Modifier
                                            .fillMaxSize()
                                            .padding(padding)
                                            .padding(16.dp)
                                    ) {
                                        FavoriteScreen(navController = navController)
                                    }
                                },
                                navController = navController
                            )
                        }
                        composable("cart") {
                            MainLayout (
                                content = { padding ->
                                    Column (
                                        modifier = Modifier
                                            .fillMaxSize()
                                            .padding(padding)
                                            .padding(16.dp)
                                    ) {
                                        CartPage(navController = navController)
                                    }
                                },
                                navController = navController
                            )
                        }
                        composable("checkout/{orderId}",
                            arguments = listOf(navArgument("orderId") { type = NavType.StringType })
                        ) { backStackEntry ->
                            val orderId = backStackEntry.arguments?.getString("orderId")
                            MainLayout(
                                content = { padding ->
                                    Column(
                                        modifier = Modifier
                                            .fillMaxSize()
                                            .padding(padding)
                                    ) {
                                        CheckoutScreen(OrderId = orderId ?: "00000")
                                    }
                                },
                                navController = navController
                            )
                        }
                    composable("product/{productId}") { backStackEntry ->
                        val productId = backStackEntry.arguments?.getString("productId") ?: ""
                        MainLayout(
                            content = { padding ->
                                Column(
                                    modifier = Modifier
                                        .fillMaxSize()
                                        .padding(padding)
                                ) {
                                    ProductDetailsScreen(productId = productId)
                                }
                            },
                            navController = navController
                        )
                    }
                        composable("catalog") {
                            MainLayout(
                                content = { padding ->
                                    Column(modifier = Modifier.padding(padding)) {
                                        CategoryRootScreen(navController = navController)
                                    }
                                },
                                navController = navController
                            )
                        }

                        composable("catalog/{slug}") { backStackEntry ->
                            val slug = backStackEntry.arguments?.getString("slug") ?: ""

                            MainLayout(
                                content = { padding ->
                                    Column(modifier = Modifier.padding(padding)) {
                                        CategorySubScreen(slug = slug, navController = navController)
                                    }
                                },
                                navController = navController
                            )
                        }

                        composable("search/{query}") { backStackEntry ->
                            val query = backStackEntry.arguments?.getString("query") ?: ""

                            MainLayout(
                                content = { padding ->
                                    Column(modifier = Modifier.padding(padding)) {
                                        SearchProducts(navController = navController, query = query)
                                    }
                                },
                                navController = navController
                            )
                        }
                    }
                }
            }
        }
    }
}

@Composable
fun LoginScreen(context: Context, onLoginSuccess: () -> Unit) {
    val viewModel = remember { LoginViewModel() }
    val state by viewModel.uiState.collectAsState()

    var email by remember { mutableStateOf("") }
    var password by remember { mutableStateOf("") }
    var isLoading by remember { mutableStateOf(false) }

    // Подписываемся на состояние и переходим при успехе
    LaunchedEffect(key1 = state) {
        if (state is LoginUiState.Success) {
            onLoginSuccess()
        }
    }

    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        Text(
            text = "Добро пожаловать в NoSTORE",
            style = MaterialTheme.typography.headlineMedium,
            modifier = Modifier.padding(bottom = 32.dp)
        )

        when (val currentState = state) {
            is LoginUiState.Error -> {
                var visible by remember { mutableStateOf(false) }
                LaunchedEffect(currentState) {
                    visible = true
                }
                AnimatedVisibility(
                    visible = visible,
                    enter = slideInVertically(
                        initialOffsetY = { -it }
                    ) + fadeIn() + scaleIn(
                        initialScale = 0.8f
                    ),
                    exit = slideOutVertically() + fadeOut() + scaleOut()
                ) {
                    Text(
                        text = currentState.message,
                        color = MaterialTheme.colorScheme.error,
                        modifier = Modifier.padding(bottom = 16.dp)
                    )
                }
            }
            LoginUiState.Loading -> {
                var visible by remember { mutableStateOf(false) }
                LaunchedEffect(Unit) {
                    visible = true
                }
                AnimatedVisibility(
                    visible = visible,
                    enter = fadeIn() + scaleIn(
                        initialScale = 0.5f
                    ),
                    exit = fadeOut() + scaleOut()
                ) {
                    CircularProgressIndicator()
                }
            }
            else -> {}
        }

        val isEmailValid = remember(email) {
            email.contains("@") && email.contains(".")
        }

        
        OutlinedTextField(
            value = email,
            onValueChange = { email = it },
            label = { Text("Электронная почта") },
            modifier = Modifier
                .fillMaxWidth()
                .padding(bottom = 16.dp),
            keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Email),
            colors = OutlinedTextFieldDefaults.colors(
                focusedBorderColor = if (isEmailValid) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.tertiary,
                unfocusedBorderColor = if (isEmailValid) MaterialTheme.colorScheme.outline else MaterialTheme.colorScheme.tertiary
            )
        )

        OutlinedTextField(
            value = password,
            onValueChange = { password = it },
            label = { Text("Пароль") },
            modifier = Modifier
                .fillMaxWidth()
                .padding(bottom = 24.dp),
            visualTransformation = PasswordVisualTransformation(),
            keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Password)
        )

        val isFormValid by remember(email, password) {
            derivedStateOf {
                email.isNotBlank() && password.isNotBlank()
            }
        }

        Button(
            onClick = {
                isLoading = true
                viewModel.login(email, password, context)
                isLoading = false
            },
            modifier = Modifier
                .fillMaxWidth()
                .height(50.dp),
            enabled = isFormValid && !isLoading
        ) {
            if (isLoading) {
                CircularProgressIndicator(
                    modifier = Modifier.size(24.dp),
                    color = MaterialTheme.colorScheme.onPrimary
                )
            } else {
                Text("Войти в аккаунт")
            }
        }
    }
}
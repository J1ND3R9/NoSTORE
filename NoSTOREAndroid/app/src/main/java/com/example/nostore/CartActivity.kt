package com.example.nostore

import CartViewModel
import ProductDto
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.lifecycle.viewmodel.compose.viewModel
import coil.compose.AsyncImage

@Composable
fun CartItem(
    productDto: ProductDto,
    quantity: Int,
    onQuantityChange: (Int) -> Unit,
    onDelete: () -> Unit,
    onFavorite: () -> Unit,
    isSelected: Boolean,
    onSelectionChange: (Boolean) -> Unit
) {
    var localQuantity by remember { mutableStateOf(quantity) }

    LaunchedEffect(quantity) {
        localQuantity = quantity
    }

    Card(
        modifier = Modifier
            .fillMaxWidth()
            .padding(8.dp),
        shape = RoundedCornerShape(8.dp)
    ) {
        Box(
            modifier = Modifier
                .fillMaxWidth()
                .padding(8.dp)
        ) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.spacedBy(8.dp)
            ) {
                // Product Image
                AsyncImage(
                    model = "http://10.0.2.2:7168/photos/products/${productDto.Name}/${productDto.Image}",
                    contentDescription = productDto.Name,
                    modifier = Modifier.size(100.dp)
                )

                // Product Details
                Column(
                    modifier = Modifier.weight(1f),
                    verticalArrangement = Arrangement.spacedBy(4.dp)
                ) {
                    Text(
                        text = productDto.Name,
                        style = MaterialTheme.typography.titleMedium,
                        fontSize = 14.sp,
                        maxLines = 2,
                        overflow = TextOverflow.Ellipsis
                    )

                    // Quantity Controls
                    Row(
                        verticalAlignment = Alignment.CenterVertically,
                        horizontalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        IconButton(
                            onClick = {
                                if (localQuantity > 1) {
                                    localQuantity--
                                    onQuantityChange(-1)
                                }
                            },
                            modifier = Modifier.size(24.dp)
                        ) {
                            Icon(Icons.Default.KeyboardArrowLeft, "Уменьшить")
                        }
                        Text(text = localQuantity.toString())
                        IconButton(
                            onClick = {
                                localQuantity++
                                onQuantityChange(1)
                            },
                            modifier = Modifier.size(24.dp)
                        ) {
                            Icon(Icons.Default.KeyboardArrowRight, "Увеличить")
                        }
                    }
                }

                // Right side buttons and prices
                Column(
                    horizontalAlignment = Alignment.End,
                    verticalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    Row(
                        horizontalArrangement = Arrangement.spacedBy(4.dp)
                    ) {
                        IconButton(
                            onClick = onFavorite,
                            modifier = Modifier.size(24.dp)
                        ) {
                            Icon(
                                Icons.Default.Favorite,
                                contentDescription = "Избранное",
                                tint = if (productDto.isFavorite)
                                    MaterialTheme.colorScheme.primary
                                else
                                    MaterialTheme.colorScheme.onSurfaceVariant
                            )
                        }
                        IconButton(
                            onClick = onDelete,
                            modifier = Modifier.size(24.dp)
                        ) {
                            Icon(
                                Icons.Default.Delete,
                                contentDescription = "Удалить",
                                tint = MaterialTheme.colorScheme.error
                            )
                        }
                    }

                    Column(
                        horizontalAlignment = Alignment.End
                    ) {
                        Text(
                            text = "${productDto.FinalPrice * localQuantity} ₽",
                            style = MaterialTheme.typography.titleMedium,
                            color = MaterialTheme.colorScheme.primary
                        )
                        if (localQuantity > 1) {
                            Text(
                                text = "${productDto.FinalPriceString}/шт.",
                                style = MaterialTheme.typography.bodySmall,
                                color = MaterialTheme.colorScheme.onSurfaceVariant
                            )
                        }
                    }
                }
            }

            // Checkbox in lower right corner
            Checkbox(
                checked = isSelected,
                onCheckedChange = onSelectionChange,
                modifier = Modifier
                    .align(Alignment.BottomEnd)
                    .padding(top = 40.dp)
            )
        }
    }
}
@Composable
fun CartPage() {
    var selectedItems by remember { mutableStateOf<Set<String>>(emptySet()) }
    val viewModel = viewModel<CartViewModel>()
    val uiState by viewModel.uiState.collectAsState()

    fun refreshCart() {
        viewModel.loadCart()
    }

    LaunchedEffect(Unit) {
        refreshCart()
    }

    val cartItems by remember {
        derivedStateOf {
            when (val state = uiState) {
                is CartUiState.Success -> state.items
                else -> emptyList()
            }
        }
    }

    LaunchedEffect(cartItems) {
        val initialSelection = cartItems
            .filter { it.isSelected }
            .map { it.productDto._id }
            .toSet()

        selectedItems = initialSelection
    }

    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp)
    ) {
        LazyColumn(
            modifier = Modifier.weight(1f),
            verticalArrangement = Arrangement.spacedBy(8.dp)
        ) {
            items(cartItems) { cart ->
                CartItem(
                    productDto = cart.productDto,
                    quantity = cart.quantity,
                    onQuantityChange = { newQuantity ->
                        viewModel.changeQuantity(cart.productDto._id, newQuantity)
                    },
                    onDelete = {
                        viewModel.removeFromCart(cart.productDto._id)

                               },
                    onFavorite = {

                    },
                    isSelected = selectedItems.contains(cart.productDto._id),
                    onSelectionChange = { isSelected ->
                        viewModel.updateSelection(cart.productDto._id, isSelected)
                        if (isSelected) {
                            selectedItems = selectedItems + cart.productDto._id
                        } else {
                            selectedItems = selectedItems - cart.productDto._id
                        }
                    }
                )
            }
        }

        // Total and Checkout Button
        Card(
            modifier = Modifier
                .fillMaxWidth()
                .padding(vertical = 16.dp)
        ) {
            Column(
                modifier = Modifier.padding(16.dp)
            ) {
                Text(
                    text = "Итого: ${cartItems.filter { selectedItems.contains(it.productDto._id) }.sumOf { it.productDto.FinalPrice * it.quantity }} ₽",
                    style = MaterialTheme.typography.titleLarge
                )
                
                Button(
                    onClick = { /* TODO: Implement checkout */ },
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(top = 16.dp),
                    enabled = selectedItems.isNotEmpty()
                ) {
                    Text("Оформить заказ")
                }
            }
        }
    }
}

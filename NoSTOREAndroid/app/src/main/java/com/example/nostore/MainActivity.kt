package com.example.nostore

import ProductDto
import android.util.Log
import androidx.compose.foundation.background
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
import androidx.compose.foundation.lazy.LazyRow
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Star
import androidx.compose.material3.Card
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.Divider
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.navigation.NavController
import androidx.navigation.compose.rememberNavController
import coil.compose.AsyncImage

@Composable
fun ProductItem(productDto: ProductDto,
                navController: NavController,
                modifier: Modifier = Modifier) {
    Card(
        modifier = modifier
            .padding(top = 16.dp, end = 16.dp),
        onClick = {
            navController.navigate("product/${productDto._id}")
        }
    ) {
        Column(modifier = Modifier.padding(12.dp)) {
            AsyncImage(
                model = "http://10.0.2.2:7168/photos/products/${productDto.Name}/${productDto.Image}",
                contentDescription = productDto.Name,
                modifier = Modifier.size(100.dp)
                    .align(alignment = Alignment.CenterHorizontally)
            )
            Spacer(Modifier.height(4.dp))
            Divider()
            Spacer(Modifier.height(4.dp))
            Text(text = productDto.Name,
                maxLines = 2,
                fontSize = 16.sp,
                overflow = TextOverflow.Ellipsis)
            Spacer(Modifier.height(14.dp))
            Row (
                horizontalArrangement = Arrangement.Center,
                verticalAlignment = Alignment.CenterVertically,
                modifier = Modifier.fillMaxWidth()
                    .background(MaterialTheme.colorScheme.background,
                        shape = RoundedCornerShape(6.dp))
                    .padding(4.dp)
            ) {
                Icon(
                    imageVector = Icons.Default.Star,
                    tint = MaterialTheme.colorScheme.primary,
                    contentDescription = "Рейтинг")
                Spacer(Modifier.width(1.dp))
                Text(
                    text = if (productDto.Rating < 1.0) "Нет оценок" else productDto.Rating.toString(),
                    color = MaterialTheme.colorScheme.primary)
            }
            Spacer(Modifier.height(14.dp))
            Text(
                text = productDto.FinalPriceString,
                fontSize = 20.sp,
                color = MaterialTheme.colorScheme.primary)
        }
    }
}

@Composable
fun RecentlyProducts(modifier: Modifier = Modifier, navController: NavController ) {
    var productDtos by remember { mutableStateOf<List<ProductDto>>(emptyList()) }
    var isLoading by remember { mutableStateOf(false) }
    var error by remember { mutableStateOf<String?>(null) }

    Box (
        modifier = Modifier.background(MaterialTheme.colorScheme.surface,
            shape = RoundedCornerShape(5.dp))
            .padding(horizontal = 30.dp, vertical = 5.dp)
    ) {
        Text(
            text = "Новые товары!",
            textAlign = TextAlign.Center,
            color = MaterialTheme.colorScheme.onSurface
        )
    }


    LaunchedEffect(Unit) {
        isLoading = true
        try {
            val data = NetworkClient.authApi.getAllProducts()
            productDtos = data
        } catch (e: Exception) {
            error = "Ошибка загрузки: ${e.message}"
        } finally {
            isLoading = false
        }
    }

    if (isLoading) {
        CircularProgressIndicator()
    } else if (error != null) {
        Text(text = error!!, color = Color.Red)
    } else {
        LazyRow {
            items(productDtos.size) { index ->
                ProductItem(productDto = productDtos[index], navController = navController, modifier = Modifier.width(180.dp))
            }
        }
    }
}

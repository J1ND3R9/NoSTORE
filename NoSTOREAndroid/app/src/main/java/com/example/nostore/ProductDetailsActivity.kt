import androidx.compose.foundation.ExperimentalFoundationApi
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxHeight
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.LazyRow
import androidx.compose.foundation.lazy.grid.GridCells
import androidx.compose.foundation.lazy.grid.LazyVerticalGrid
import androidx.compose.foundation.pager.HorizontalPager
import androidx.compose.foundation.pager.PagerState
import androidx.compose.foundation.pager.rememberPagerState
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Favorite
import androidx.compose.material.icons.filled.Share
import androidx.compose.material.icons.filled.Star
import androidx.compose.material.icons.outlined.Favorite
import androidx.compose.material.icons.outlined.Star
import androidx.compose.material3.Button
import androidx.compose.material3.Card
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.Divider
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.text.style.TextDecoration
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import coil.compose.AsyncImage
import com.example.nostore.ProductUiState
import com.example.nostore.ProductViewModel
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.draw.clip
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.sp
import com.example.nostore.ui.theme.Strong
import java.text.SimpleDateFormat
import java.util.Locale

@Composable
fun ProductDetailsScreen(productId: String) {
    val viewModel = viewModel<ProductViewModel>()
    val uiState by viewModel.uiState.collectAsState()

    LaunchedEffect(productId) {
        viewModel.loadProduct(productId)
    }

    when (val state = uiState) {
        is ProductUiState.Loading -> {
            Box(modifier = Modifier.fillMaxSize()) {
                CircularProgressIndicator(
                    modifier = Modifier.align(Alignment.Center)
                )
            }
        }
        is ProductUiState.Success -> {
            ProductDetailsContent(product = state.status)
        }
        is ProductUiState.Error -> {
            Box(modifier = Modifier.fillMaxSize()) {
                Text(
                    text = state.message,
                    color = MaterialTheme.colorScheme.error,
                    modifier = Modifier.align(Alignment.Center)
                )
            }
        }
    }
}

@OptIn(ExperimentalFoundationApi::class)
@Composable
fun ProductImageSlider(images: List<String>?, productName: String) {
    val pagerState = rememberPagerState { images?.size ?: 0 }

    Box(
        modifier = Modifier
            .fillMaxWidth()
            .height(300.dp)
    ) {
        HorizontalPager(
            state = pagerState,
            modifier = Modifier.fillMaxSize()
        ) { page ->
            AsyncImage(
                model = "http://10.0.2.2:7168/photos/products/${productName}/${images?.get(page)}",
                contentDescription = productName,
                modifier = Modifier.fillMaxSize(),
                contentScale = ContentScale.Fit
            )
        }

        Row(
            modifier = Modifier
                .align(Alignment.BottomCenter)
                .padding(16.dp),
            horizontalArrangement = Arrangement.spacedBy(4.dp)
        ) {
            repeat(pagerState.pageCount) { iteration ->
                val color = if (pagerState.currentPage == iteration) {
                    MaterialTheme.colorScheme.primary
                } else {
                    MaterialTheme.colorScheme.onSurface.copy(alpha = 0.3f)
                }
                Box(
                    modifier = Modifier
                        .size(8.dp)
                        .clip(CircleShape)
                        .background(color)
                )
            }
        }
    }
}

@Composable
fun ProductCharacteristics(properties: Map<String, List<Map<String, String>>>) {
    Column {
        properties.forEach { (category, propsList) ->
            Text(
                text = category,
                style = MaterialTheme.typography.titleMedium,
                modifier = Modifier.padding(vertical = 8.dp),
                color = MaterialTheme.colorScheme.primary,
                fontFamily = Strong,
                fontSize = 24.sp

            )

            propsList.forEach { prop ->
                val (name, value) = prop.entries.firstOrNull() ?: return@forEach

                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(vertical = 2.dp),
                    horizontalArrangement = Arrangement.SpaceBetween
                ) {
                    Text(
                        text = name,
                        style = MaterialTheme.typography.bodyMedium,
                        modifier = Modifier.weight(1f),
                        fontFamily = Strong,
                        color = MaterialTheme.colorScheme.secondary,
                        fontSize = 18.sp
                    )
                    Text(
                        text = if (value == "True") "есть" else if (value == "False") "отсутствует" else value,
                        style = MaterialTheme.typography.bodyMedium,
                        modifier = Modifier.weight(1f),
                        textAlign = TextAlign.End,
                        fontFamily = Strong,
                        color = MaterialTheme.colorScheme.onSurface,
                        fontSize = 18.sp
                    )
                }
            }

            Divider(modifier = Modifier.padding(vertical = 8.dp))
        }
    }
}

@Composable
fun LocalizedDate(dateString: String?): String {
    if (dateString == null) return ""
    return try {
        val inputFormat = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.getDefault())
        val outputFormat = SimpleDateFormat("dd MMMM yyyy", Locale("ru"))
        val date = inputFormat.parse(dateString)
        outputFormat.format(date)
    } catch (e: Exception) {
        dateString
    }
}

@Composable
fun ProductDetailsContent(product: ProductDto) {
    val scrollState = rememberScrollState()
    val viewModel = viewModel<CartViewModel>()

    Column(
        modifier = Modifier
            .fillMaxSize()
            .verticalScroll(scrollState)
            .padding(16.dp)
    ) {
        // 1. Image
        ProductImageSlider(images = product.Images, productName = product.Name)

        Spacer(modifier = Modifier.height(16.dp))

        // 2. Product Name
        Text(
            text = product.Name,
            style = MaterialTheme.typography.headlineMedium,
            fontFamily = Strong
            )

        Spacer(modifier = Modifier.height(8.dp))

        // 3. Action Buttons
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.SpaceEvenly
        ) {
            IconButton(onClick = {}) {
                Icon(Icons.Default.Favorite, contentDescription = "В избранное")
            }
            IconButton(onClick = {}) {
                Icon(Icons.Default.Share, contentDescription = "Сравнить")
            }
            Button(onClick = { viewModel.addToCart(product._id) }, modifier = Modifier.weight(1f)) {
                Text("В корзину",
                    fontFamily = Strong,
                )
            }
        }

        Spacer(modifier = Modifier.height(16.dp))
        Divider()
        Spacer(modifier = Modifier.height(16.dp))

        // 4. Description
        Text(
            text = "Описание",
            style = MaterialTheme.typography.titleLarge
        )
        Text(
            text = product.Description,
            style = MaterialTheme.typography.bodyMedium,
            modifier = Modifier.padding(vertical = 8.dp)
        )

        Spacer(modifier = Modifier.height(16.dp))

        // 5. Characteristics (простая сетка без LazyGrid)
        Text(
            text = "Характеристики",
            style = MaterialTheme.typography.titleLarge
        )
        ProductCharacteristics(product.Properties)
        Spacer(modifier = Modifier.height(16.dp))

        // 6. Reviews
        Text(
            text = "Отзывы",
            style = MaterialTheme.typography.titleLarge
        )
        val sortedReviews = product.Reviews?.sortedByDescending { it.likes }
        sortedReviews?.forEach { review ->
            Column(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(vertical = 8.dp)
            ) {
                // Header with nickname and likes
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Row {
                        AsyncImage(
                            model = "http://10.0.2.2:7168/photos/avatars/" + if (!review.user.avatarExt.isNullOrEmpty()) "${review.user.id}${review.user.avatarExt}" else "default.png",
                            contentDescription = review.user.avatarExt,
                            contentScale = ContentScale.Crop,
                            modifier = Modifier
                                .size(40.dp)
                                .clip(CircleShape)
                        )
                        Text(
                            text = review.user.nickname ?: "Анонимный пользователь",
                            style = MaterialTheme.typography.bodyLarge
                        )
                    }

                    Row(
                        verticalAlignment = Alignment.CenterVertically,
                        horizontalArrangement = Arrangement.spacedBy(4.dp)
                    ) {
                        Text(
                            text = review.likes.toString(),
                            style = MaterialTheme.typography.bodyMedium
                        )
                        IconButton(
                            onClick = { /* TODO: Implement like functionality */ }
                        ) {
                            Icon(
                                imageVector = if (review.usersLikes?.contains(review.userId) == true)
                                    Icons.Filled.Favorite else Icons.Outlined.Favorite,
                                contentDescription = "Like",
                                tint = if (review.usersLikes?.contains(review.userId) == true)
                                    MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurface
                            )
                        }
                    }
                }

                // Date
                Text(
                    text = LocalizedDate(review.createDate),
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )

                // Rating stars
                Row(
                    horizontalArrangement = Arrangement.spacedBy(2.dp)
                ) {
                        Icon(
                            imageVector = Icons.Outlined.Star,
                            contentDescription = "Star",
                            modifier = Modifier.size(16.dp)
                        )
                    Text(
                        text = "${review.rating} из 5"
                    )
                }

                // Usage duration
                if (!review.usageTime.isNullOrEmpty()) {
                    Text(
                        text = "Использовал: ${review.usageTime}",
                        style = MaterialTheme.typography.bodyMedium,
                        modifier = Modifier.padding(vertical = 4.dp)
                    )
                }

                // Pros
                if (!review.pluses.isNullOrEmpty()) {
                    Text(
                        text = "Достоинства:",
                        style = MaterialTheme.typography.bodyMedium,
                        fontWeight = FontWeight.Bold,
                        color = MaterialTheme.colorScheme.secondary
                    )
                    Text(
                        text = review.pluses,
                        style = MaterialTheme.typography.bodyMedium
                    )
                }

                // Cons
                if (!review.minuses.isNullOrEmpty()) {
                    Text(
                        text = "Недостатки:",
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.tertiary
                    )
                    Text(
                        text = review.minuses,
                        style = MaterialTheme.typography.bodyMedium
                    )
                }

                // Review text
                Text(
                    text = review.comment ?: "",
                    style = MaterialTheme.typography.bodyMedium,
                    modifier = Modifier.padding(vertical = 4.dp)
                )

                // Additions
                if (!review.additions.isNullOrEmpty()) {
                    review.additions.forEach { addition ->
                            Text(
                                text = "Дополнение от ${LocalizedDate(addition.date)}",
                                style = MaterialTheme.typography.bodySmall,
                                color = MaterialTheme.colorScheme.secondary
                            )
                            Text(
                                text = addition.text ?: "",
                                style = MaterialTheme.typography.bodyMedium
                            )
                    }
                }

                Divider(modifier = Modifier.padding(vertical = 8.dp))
            }
        }

        Spacer(modifier = Modifier.height(32.dp))
    }
}
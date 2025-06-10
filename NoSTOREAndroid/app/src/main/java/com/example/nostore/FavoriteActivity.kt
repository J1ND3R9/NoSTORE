import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.derivedStateOf
import androidx.compose.runtime.getValue
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavController
import com.example.nostore.ProductItemColumn

@Composable
fun FavoriteScreen(modifier: Modifier = Modifier, navController: NavController ) {
    val viewModel: CartViewModel = viewModel()
    val uiState by viewModel.uiState.collectAsState()

    val favoriteItems by remember {
        derivedStateOf {
            when (uiState) {
                is CartUiState.SuccessListProducts -> (uiState as CartUiState.SuccessListProducts).items
                else -> emptyList()
            }
        }
    }

    Box (
        modifier = Modifier.background(MaterialTheme.colorScheme.surface,
            shape = RoundedCornerShape(5.dp))
            .padding(horizontal = 30.dp, vertical = 5.dp)
    ) {
        Text(
            text = "В вашем избранном ${favoriteItems.size} товаров на ${favoriteItems.sumOf { s -> s.FinalPrice }}",
            textAlign = TextAlign.Center,
            color = MaterialTheme.colorScheme.onSurface
        )
    }


    fun refreshFav() {
        viewModel.loadFavs()
    }

    LaunchedEffect(Unit) {
        refreshFav()
    }

    val favItems by remember {
        derivedStateOf {
            when (val state = uiState) {
                is CartUiState.SuccessListProducts -> state.items
                else -> emptyList()
            }
        }
    }

    when (uiState) {
        is CartUiState.Loading -> {
            Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
                CircularProgressIndicator()
            }
        }
        is CartUiState.Error -> {
            val message = (uiState as CartUiState.Error).message
            Text(text = message, modifier = Modifier.padding(16.dp), color = MaterialTheme.colorScheme.error)
        }
        is CartUiState.SuccessListProducts -> {
            LazyColumn {
                items(favoriteItems) { product ->
                    ProductItemColumn(productDto = product, navController = navController)
                }
            }
        }
        else -> {}
    }
}
package com.example.nostore

import FilterViewModel
import ProductDto
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
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.grid.GridCells
import androidx.compose.foundation.lazy.grid.LazyVerticalGrid
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Favorite
import androidx.compose.material.icons.filled.FavoriteBorder
import androidx.compose.material.icons.filled.KeyboardArrowDown
import androidx.compose.material.icons.filled.KeyboardArrowUp
import androidx.compose.material.icons.filled.List
import androidx.compose.material.icons.filled.Search
import androidx.compose.material.icons.filled.ShoppingCart
import androidx.compose.material.icons.filled.Star
import androidx.compose.material3.Button
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.Checkbox
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.TextField
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavController
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.navigation.compose.rememberNavController
import coil.compose.AsyncImage


@Composable
fun CategoryRootScreen(navController: NavController) {
    val viewModel = viewModel<CategoryViewModel>()

    val uiState by viewModel.uiState.collectAsState()

    LaunchedEffect(Unit) {
        viewModel.loadRootCategories()
    }

    when (val state = uiState) {
        is CategoryUiState.Loading -> {
            Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
                CircularProgressIndicator()
            }
        }
        is CategoryUiState.Categories -> {
            CategoryList(
                title = "Выберите категорию",
                categories = state.items,
                onCategoryClick = { slug ->
                    navController.navigate("catalog/$slug")
                }
            )
        }
        is CategoryUiState.Error -> {
            Text(text = state.message, modifier = Modifier.padding(16.dp))
        }
        else -> {}
    }
}

@Composable
fun CategorySubScreen(slug: String, navController: NavController) {
    val viewModel = viewModel<CategoryViewModel>()

    val uiState by viewModel.uiState.collectAsState()

    LaunchedEffect(slug) {
        viewModel.navigateTo(slug)
    }

    when (val state = uiState) {
        is CategoryUiState.Loading -> {
            Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
                CircularProgressIndicator()
            }
        }
        is CategoryUiState.Subcategories -> {
            CategoryList(
                title = "Подкатегории: ${state.name}",
                categories = state.subcategories,
                onCategoryClick = { nextSlug ->
                    navController.navigate("catalog/$nextSlug")
                }
            )
        }
        is CategoryUiState.ShowProducts -> {
            ProductListScreen(products = state.products, filters = state.filters, navController = navController)
        }
        is CategoryUiState.Error -> {
            Text(text = state.message, modifier = Modifier.padding(16.dp))
        }
        else -> {}
    }
}

@Composable
fun CategoryScreen(viewModel: CategoryViewModel = viewModel(), navController: NavController) {
    val state by viewModel.uiState.collectAsState()

    when (val current = state) {
        is CategoryUiState.Loading -> {
            Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
                CircularProgressIndicator()
            }
        }
        is CategoryUiState.Categories -> {
            CategoryList(
                title = "Выберите категорию",
                categories = current.items,
                onCategoryClick = { slug ->
                    viewModel.navigateTo(slug)
                }
            )
        }
        is CategoryUiState.Subcategories -> {
            CategoryList(
                title = "Подкатегории ${current.name}",
                categories = current.subcategories,
                onCategoryClick = { slug ->
                    viewModel.navigateTo(slug)
                }
            )
        }
        is CategoryUiState.ShowProducts -> {
            ProductListScreen(products = current.products, filters = current.filters, navController)
        }
        is CategoryUiState.Error -> {
            Text(text = current.message, modifier = Modifier.padding(16.dp))
        }
    }
}

@Composable
fun CategoryList(title: String, categories: List<CategoryDto>, onCategoryClick: (String) -> Unit) {
    Column(modifier = Modifier.fillMaxSize().padding(16.dp)) {
        Text(text = title, style = MaterialTheme.typography.headlineMedium)
        LazyVerticalGrid(columns = GridCells.Adaptive(minSize = 128.dp)) {
            items(categories.size) { index ->
                val category = categories[index]
                CategoryItem(category = category, onClick = {
                    onCategoryClick(category.Slug)
                })
            }
        }
    }
}

@Composable
fun CategoryItem(category: CategoryDto, onClick: () -> Unit) {
    Card(
        modifier = Modifier
            .padding(8.dp)
            .fillMaxWidth(),
        onClick = onClick
    ) {
        Column(horizontalAlignment = Alignment.CenterHorizontally) {
            AsyncImage(
                model = "http://10.0.2.2:7168/photos/categories/${category.Image}",
                contentDescription = category.Name,
                modifier = Modifier.size(96.dp)
            )
            Text(
                text = category.Name,
                textAlign = TextAlign.Center,
                modifier = Modifier.padding(8.dp)
            )
        }
    }
}

@Composable
fun FilterBlock(filters: FilterDto?, viewModel: FilterViewModel) {
    Card(
        modifier = Modifier
            .fillMaxWidth()
            .padding(16.dp)
    ) {
        Column(
            modifier = Modifier
                .padding(16.dp)
                .verticalScroll(rememberScrollState())
        ) {
            Text(text = "Фильтры", style = MaterialTheme.typography.titleLarge)

            PriceFilterSection(viewModel)

            var applyButtonVisible by remember { mutableStateOf(false) }
            if (applyButtonVisible) {
                Button(
                    onClick = { viewModel.applyFilters(filters?.category ?: "Неизвестная категория") },
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(top = 8.dp)
                ) {
                    Text(text = "Применить")
                }
            }

            filters?.properties?.forEach { (groupName, properties) ->
                properties.forEach { (key, values) ->
                    FilterGroupSection(
                        groupName = groupName,
                        propertyName = key,
                        values = values
                    ) { selectedValues ->
                        val updatedFilters = viewModel.state.value.filters.toMutableMap().apply {
                            this[key] = selectedValues
                        }
                        viewModel.updateFilters(updatedFilters)
                        applyButtonVisible = true
                    }
                }
            }
        }
    }
}

@Composable
fun FilterGroupSection(
    groupName: String,
    propertyName: String,
    values: List<String>,
    onApply: (Map<String, List<String>>) -> Unit
) {
    var expanded by remember { mutableStateOf(true) }
    var checkedItems by remember { mutableStateOf(setOf<String>()) }

    Card(
        modifier = Modifier.fillMaxWidth().padding(vertical = 4.dp),
        elevation = CardDefaults.cardElevation(defaultElevation = 4.dp)
    ) {
        Column(modifier = Modifier.padding(12.dp)) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                Text(
                    text = propertyName.removeSuffix("s"),
                    style = MaterialTheme.typography.titleMedium
                )
                IconButton(onClick = { expanded = !expanded }) {
                    Icon(
                        imageVector = if (expanded) Icons.Default.KeyboardArrowUp else Icons.Default.KeyboardArrowDown,
                        contentDescription = if (expanded) "Скрыть" else "Развернуть"
                    )
                }
            }

            if (expanded) {
                var searchQuery by remember { mutableStateOf("") }

                TextField(
                    value = searchQuery,
                    onValueChange = { searchQuery = it },
                    label = { Text("Поиск") },
                    modifier = Modifier.fillMaxWidth().padding(vertical = 8.dp),
                    singleLine = true,
                    leadingIcon = { Icon(Icons.Default.Search, null) }
                )

                values
                    .filter { it.contains(searchQuery, ignoreCase = true) || searchQuery.isBlank() }
                    .forEach { item ->
                        Row(
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Checkbox(
                                checked = checkedItems.contains(item),
                                onCheckedChange = { isChecked ->
                                    checkedItems = if (isChecked) {
                                        checkedItems + item
                                    } else {
                                        checkedItems - item
                                    }
                                    onApply(mapOf(propertyName to checkedItems.toList()))
                                }
                            )
                            Text(
                                text = when (item) {
                                    "True" -> "есть"
                                    "False" -> "нет"
                                    else -> item
                                }
                            )
                        }
                    }

                Row(
                    modifier = Modifier.fillMaxWidth().padding(top = 8.dp),
                    horizontalArrangement = Arrangement.SpaceEvenly
                ) {
                    if (values.size > 6) {
                        TextButton(onClick = { /* Показать всё */ }) {
                            Text(text = "Показать всё")
                        }
                    }
                    TextButton(onClick = { checkedItems = emptySet() }) {
                        Text(text = "Сбросить")
                    }
                }
            }
        }
    }
}

@Composable
fun PriceFilterSection(viewModel: FilterViewModel) {
    var minPrice by remember { mutableStateOf("") }
    var maxPrice by remember { mutableStateOf("") }

    Column {
        Text(text = "Цена", style = MaterialTheme.typography.titleMedium)

        Row(
            modifier = Modifier.padding(vertical = 8.dp),
            horizontalArrangement = Arrangement.spacedBy(8.dp)
        ) {
            OutlinedTextField(
                value = minPrice,
                onValueChange = {
                    minPrice = it
                    viewModel.updateMinPrice(it)
                },
                label = { Text("От") },
                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                modifier = Modifier.weight(1f)
            )
            OutlinedTextField(
                value = maxPrice,
                onValueChange = {
                    maxPrice = it
                    viewModel.updateMaxPrice(it)
                },
                label = { Text("До") },
                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                modifier = Modifier.weight(1f)
            )
        }

        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.End
        ) {
            TextButton(onClick = {
                minPrice = ""
                maxPrice = ""
                viewModel.updateMinPrice("")
                viewModel.updateMaxPrice("")
            }) {
                Text(text = "Сбросить")
            }
        }
    }
}

@Composable
fun ProductListScreen(
    products: List<ProductDto>,
    filters: FilterDto?,
    navController: NavController
) {
    val repository = remember { ProductRepository() }
    val viewModel: FilterViewModel = viewModel(key = "filter_vm") {
        FilterViewModel(repository).apply {
            // Начальное состояние — исходные товары
            updateProducts(products)
        }
    }

    var showFilters by remember { mutableStateOf(false) }

    Column(modifier = Modifier.fillMaxSize()) {
        Box(modifier = Modifier.fillMaxWidth()) {
            IconButton(
                onClick = { showFilters = !showFilters },
                modifier = Modifier.align(Alignment.TopEnd)
            ) {
                Icon(Icons.Default.List, contentDescription = "Filter")
            }
        }

        if (showFilters) {
            FilterBlock(filters = filters, viewModel = viewModel)
        }

        // 🔥 Выводим товары из ViewModel
        val uiState by viewModel.state.collectAsState()
        ProductsListContent(uiState.products, navController)
    }
}

@Composable
fun ProductsListContent(products: List<ProductDto>,
                        navController: NavController) {
    LazyColumn(modifier = Modifier.fillMaxSize()) {
        items(products) { product ->
            ProductItemColumn(
                productDto = product,
                navController = navController,
                modifier = Modifier.fillMaxWidth()
            )
        }
    }
}

@Composable
fun ProductItemColumn(productDto: ProductDto,
                      navController: NavController,
                      modifier: Modifier = Modifier) {
    Card(
        modifier = modifier.run {
            fillMaxWidth()
                .padding(all = 15.dp)
        },
        onClick = {
            navController.navigate("product/${productDto._id}")
        }
    ) {
        Box(
            modifier = Modifier.padding(16.dp)
        ) {
            // Rating and delivery time in top right
            Column(
                modifier = Modifier
                    .align(Alignment.TopStart)
            ) {
                Row(
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Icon(
                        imageVector = Icons.Default.Star,
                        contentDescription = "Rating",
                        tint = MaterialTheme.colorScheme.primary,
                        modifier = Modifier.size(16.dp)
                    )
                    Spacer(modifier = Modifier.width(4.dp))
                    Text(
                        text = if (productDto.Rating > 1) productDto.Rating.toString() else "Нет оценок",
                        style = MaterialTheme.typography.bodyMedium
                    )
                }
                Text(
                    text = "Доставка: 1 день",
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }

            // Main content
            Row(
                modifier = Modifier.fillMaxWidth()
            ) {
                // Product image
                AsyncImage(
                    model = "http://10.0.2.2:7168/photos/products/${productDto.Name}/${productDto.Image}",
                    contentDescription = productDto.Name,
                    modifier = Modifier
                        .size(110.dp)
                        .align(Alignment.CenterVertically)
                )

                Spacer(modifier = Modifier.width(16.dp))

                // Product details
                Column(
                    modifier = Modifier.weight(1f)
                ) {
                    Text(
                        text = productDto.Name,
                        style = MaterialTheme.typography.titleMedium
                    )
                    
                    Spacer(modifier = Modifier.height(4.dp))
                    
                    Text(
                        text = productDto.Tags,
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )

                    Spacer(modifier = Modifier.height(16.dp))

                    // Price and actions
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.SpaceBetween,
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Text(
                            text = productDto.FinalPriceString,
                            style = MaterialTheme.typography.titleLarge,
                            color = MaterialTheme.colorScheme.primary
                        )

                        Row {
                            IconButton(
                                onClick = { /* TODO: Add to cart */ }
                            ) {
                                Icon(
                                    imageVector = Icons.Default.ShoppingCart,
                                    tint = if (productDto.inCart) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.secondary,
                                    contentDescription = "Add to cart"
                                )
                            }
                            
                            IconButton(
                                onClick = { /* TODO: Add to favorites */ }
                            ) {
                                Icon(
                                    imageVector = Icons.Default.Favorite,
                                    contentDescription = "Add to favorites",
                                    tint = if (productDto.isFavorite) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.secondary,
                                )
                            }
                        }
                    }
                }
            }
        }
    }
}

@Composable
fun ProductItemPreview() {
    // Заглушка для NavController
    val fakeNavController = rememberNavController()

    // Заглушка для продукта
    val fakeProduct = ProductDto(
        _id = "67e52d8747728e9086eb0a62",
        Name = "Видеокарта MSI GeForce RTX 4060 VENTUS 3X OC\n",
        SEOName = "",
        Tags = "[PCIe 4.0, 8 ГБ, GDDR6, HDMI, 3 x DisplayPort, GPU 1830 МГц, 128 бит]",
        Price = 199999,
        FinalPrice = 199999,
        Discount = 0,
        Image = "0.png",
        Images = emptyList(),
        Quantity = 2354,
        Rating = 4.2,
        Reviews = emptyList(),
        FinalPriceString = "199 999 Р",
        Description = "",
        Properties = emptyMap(),
        isFavorite = false,
        inCart = true,
        inCompare = true
    )

    ProductItemColumn(
        productDto = fakeProduct,
        navController = fakeNavController
    )
}
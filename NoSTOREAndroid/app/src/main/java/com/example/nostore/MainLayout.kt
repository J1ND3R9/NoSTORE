package com.example.nostore

import android.content.Context
import androidx.activity.ComponentActivity
import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.text.BasicTextField
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ExitToApp
import androidx.compose.material.icons.filled.ExitToApp
import androidx.compose.material.icons.filled.Favorite
import androidx.compose.material.icons.filled.Home
import androidx.compose.material.icons.filled.List
import androidx.compose.material.icons.filled.Menu
import androidx.compose.material.icons.filled.Search
import androidx.compose.material.icons.filled.Settings
import androidx.compose.material.icons.filled.ShoppingCart
import androidx.compose.material3.Divider
import androidx.compose.material3.DrawerValue
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.LocalTextStyle
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.ModalDrawerSheet
import androidx.compose.material3.ModalNavigationDrawer
import androidx.compose.material3.NavigationDrawerItem
import androidx.compose.material3.NavigationDrawerItemDefaults
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.material3.rememberDrawerState
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.SolidColor
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.navigation.NavController
import androidx.navigation.compose.rememberNavController
import kotlinx.coroutines.launch

@Composable
fun Logo(
    modifier: Modifier = Modifier
) {
    Image(
        painter = painterResource(id = R.drawable.ic_logo),
        contentDescription = null,
        modifier = modifier.size(80.dp)
    )
}

@Composable
fun SearchField(

) {
    var text by remember {
        mutableStateOf("")
    }
    Box (
        modifier = Modifier
            .height(40.dp)
            .background(
                color = MaterialTheme.colorScheme.background,
                shape = RoundedCornerShape(8.dp)
            )
            .border(
                width = 1.dp,
                color = MaterialTheme.colorScheme.secondary,
                shape = RoundedCornerShape(8.dp)
            )
            .padding(horizontal = 8.dp, vertical = 4.dp),
        contentAlignment = Alignment.CenterStart
    ) {
        BasicTextField (
            value = text,
            onValueChange = { text = it },
            singleLine = true,
            textStyle = LocalTextStyle.current.copy(
                color = MaterialTheme.colorScheme.onSurface,
                fontSize = 16.sp
            ),
            cursorBrush = SolidColor(Color.Gray),
            decorationBox = { innerTextField ->
                Row(
                    verticalAlignment = Alignment.CenterVertically,
                    horizontalArrangement = Arrangement.Start,
                    modifier = Modifier.fillMaxSize()
                ) {
                    Spacer(modifier = Modifier.width(8.dp))
                    Box (modifier = Modifier.weight(1f)) {
                        if (text.isEmpty()) {
                            Text(
                                text = "Искать по сайту",
                                color = Color.Gray,
                                fontSize = 16.sp
                            )
                        }
                        innerTextField()
                    }
                    Icon (
                        imageVector = Icons.Default.Search,
                        contentDescription = "Поиск",
                        tint = MaterialTheme.colorScheme.onSurface,
                        modifier = Modifier.size(20.dp)
                    )
                }
            }
        )
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun MainLayout(
    selectedItem: Int = 0,
    onItemSelected: (Int) -> Unit = {},
    content: @Composable (PaddingValues) -> Unit,
    navController: NavController,
    context: Context = LocalContext.current
) {
    val drawerState = rememberDrawerState(initialValue = DrawerValue.Closed)
    val scope = rememberCoroutineScope()

    val menuItems = listOf("Главная", "Каталог", "Корзина", "Избранное", "Настройки")

    ModalNavigationDrawer(
        drawerState = drawerState,
        drawerContent = {
            ModalDrawerSheet(
                modifier = Modifier.width(240.dp)
            ) {
                Column(
                    modifier = Modifier
                        .fillMaxSize()
                        .background(MaterialTheme.colorScheme.surface)
                ) {
                    // Header
                    Box(
                        modifier = Modifier
                            .fillMaxWidth()
                            .background(MaterialTheme.colorScheme.primary)
                            .padding(16.dp)
                    ) {
                        Text(
                            "Меню",
                            style = MaterialTheme.typography.titleLarge,
                            color = MaterialTheme.colorScheme.onPrimary
                        )
                    }
                    
                    // Menu Items
                    Column(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(vertical = 8.dp)
                    ) {
                        menuItems.forEachIndexed { index, item ->
                            NavigationDrawerItem(
                                label = { 
                                    Text(
                                        item,
                                        style = MaterialTheme.typography.bodyLarge
                                    ) 
                                },
                                selected = selectedItem == index,
                                icon = {
                                    Icon(
                                        imageVector = when (index) {
                                            0 -> Icons.Default.Home
                                            1 -> Icons.Default.List
                                            2 -> Icons.Default.ShoppingCart
                                            3 -> Icons.Default.Favorite
                                            4 -> Icons.Default.Settings
                                            else -> Icons.Default.Home
                                        },
                                        contentDescription = null
                                    )
                                },
                                onClick = {
                                    when (index) {
                                        0 -> navController.navigate("home")
                                        1 -> navController.navigate("catalog")
                                        2 -> navController.navigate("cart")
                                        3 -> navController.navigate("favorites")
                                        4 -> navController.navigate("settings")
                                    }
                                    onItemSelected(index)
                                    scope.launch { drawerState.close() }
                                },
                                colors = NavigationDrawerItemDefaults.colors(
                                    selectedContainerColor = MaterialTheme.colorScheme.primaryContainer,
                                    unselectedContainerColor = Color.Transparent
                                )
                            )
                        }
                    }
                    
                    Spacer(modifier = Modifier.weight(1f))
                    
                    // Logout Button
                    Divider()
                    NavigationDrawerItem(
                        label = { 
                            Text(
                                "Выйти",
                                style = MaterialTheme.typography.bodyLarge,
                                color = MaterialTheme.colorScheme.error
                            ) 
                        },
                        selected = false,
                        icon = {
                            Icon(
                                imageVector = Icons.Default.ExitToApp,
                                contentDescription = null,
                                tint = MaterialTheme.colorScheme.error
                            )
                        },
                        onClick = {
                            TokenManager.clear(context)
                            navController.navigate("login")
                            scope.launch { drawerState.close() }
                        },
                        colors = NavigationDrawerItemDefaults.colors(
                            unselectedContainerColor = Color.Transparent
                        )
                    )
                }
            }
        }
    ) {
        Scaffold(
            topBar = {
                TopAppBar(
                    title = {
                        Row(
                            horizontalArrangement = Arrangement.spacedBy(20.dp),
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            Logo()
                            SearchField()
                        }
                    },
                    navigationIcon = {
                        IconButton(onClick = {
                            scope.launch { drawerState.open() }
                        }) {
                            Icon(Icons.Default.Menu, "Открыть меню")
                        }
                    },
                    actions = {
                        IconButton(onClick = { navController.navigate("cart") }) {
                            Icon(Icons.Default.ShoppingCart, "Корзина")
                        }
                    }
                )
            }
        ) { padding ->
            content(padding)
        }
    }
}

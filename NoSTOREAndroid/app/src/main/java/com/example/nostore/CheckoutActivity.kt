package com.example.nostore

import ReceiptViewModel
import ReceiptViewModelFactory
import android.graphics.Color
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.width
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.KeyboardArrowDown
import androidx.compose.material3.Button
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.unit.dp
import androidx.compose.runtime.getValue
import androidx.lifecycle.viewmodel.compose.viewModel

@Composable
fun CheckoutScreen(OrderId: String) {

    val context = LocalContext.current

    val receiptViewModel: ReceiptViewModel = viewModel(
        factory = ReceiptViewModelFactory(context)
    )


    Column {
        Text(
            text = "Спасибо за заказ!\nНомер заказа: $OrderId."
        )
        ReceiptDownloadButton(orderId = OrderId, viewModel = receiptViewModel)
    }
}

@Composable
fun ReceiptDownloadButton(
    orderId: String,
    viewModel: ReceiptViewModel
) {
    val downloadState by viewModel.downloadState.collectAsState()

    Column {
        Button(onClick = {
            viewModel.downloadReceipt(orderId)
        }) {
            Icon(Icons.Default.KeyboardArrowDown, contentDescription = "Download receipt")
            Spacer(Modifier.width(8.dp))
            Text("Скачать чек")
        }

        // Вывод результата
        when {
            downloadState.isSuccess && downloadState.getOrNull()?.isNotEmpty() == true -> {
                Text(
                    text = "Чек сохранён: ${downloadState.getOrNull()}",
                    color = MaterialTheme.colorScheme.primary,
                    style = MaterialTheme.typography.bodySmall
                )
            }
            downloadState.isFailure -> {
                Text(
                    text = "Ошибка: ${downloadState.exceptionOrNull()?.message}",
                    color = MaterialTheme.colorScheme.tertiary,
                    style = MaterialTheme.typography.bodySmall
                )
            }
        }
    }
}
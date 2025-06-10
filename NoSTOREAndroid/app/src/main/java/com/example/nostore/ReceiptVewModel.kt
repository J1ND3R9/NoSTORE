import android.content.Context
import androidx.compose.material3.Button
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.platform.LocalContext
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewModelScope
import com.example.nostore.NetworkClient.authApi
import com.example.nostore.ReceiptDownloader
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

class ReceiptViewModelFactory(
    private val context: Context
) : ViewModelProvider.Factory {

    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        if (modelClass.isAssignableFrom(ReceiptViewModel::class.java)) {
            return ReceiptViewModel(context) as T
        }
        throw IllegalArgumentException("Unknown ViewModel class")
    }
}

class ReceiptViewModel(private val context: Context) : ViewModel() {
    private val _downloadState = MutableStateFlow<Result<String>>(Result.success(""))
    val downloadState: StateFlow<Result<String>> = _downloadState

    private val downloader by lazy {
        ReceiptDownloader(context)
    }

    fun downloadReceipt(orderId: String) {
        viewModelScope.launch {
            val result = downloader.downloadReceipt(orderId, authApi)
            _downloadState.value = result
        }
    }
}
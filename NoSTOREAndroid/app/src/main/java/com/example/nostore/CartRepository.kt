    import com.example.nostore.ApiService
    import com.example.nostore.CartApiDto
    import com.example.nostore.CartItemApiDto
    import com.example.nostore.NetworkClient
    import com.example.nostore.ProductRequest
    import com.example.nostore.ProductStatus
    import kotlinx.coroutines.Dispatchers
    import kotlinx.coroutines.withContext

    class CartRepository {
        private val api = NetworkClient.authApi

        suspend fun getProductStatus(productId: String): Result<ProductStatus> =
            withContext(Dispatchers.IO) {
                try {
                    val response = api.getProductStatus(productId)
                    if (response.isSuccessful && response.body() != null) {
                        Result.success(response.body()!!)
                    } else {
                        Result.failure(Exception("Ошибка загрузки статуса продукта"))
                    }
                } catch (e: Exception) {
                    Result.failure(e)
                }
            }

        suspend fun getCart(): Result<CartApiDto> = withContext(Dispatchers.IO) {
            try {
                val response = api.getCart()
                if (response.isSuccessful && response.body() != null) {
                    Result.success(response.body()!!)
                } else {
                    Result.failure(Exception("Ошибка загрузки корзины"))
                }
            } catch (e: Exception) {
                Result.failure(e)
            }
        }

        suspend fun addFavorite(productId: String): Result<Unit> =
            withContext(Dispatchers.IO) {
                try {
                    val response = api.addFavoriteProduct(ProductRequest(ProductId = productId))
                    if (response.isSuccessful) {
                        Result.success(Unit)
                    } else {
                        Result.failure(Exception("Ошибка добавления в избранное"))
                    }
                } catch (e: Exception) {
                    Result.failure(e)
                }
            }

        suspend fun removeFavorite(productId: String): Result<Unit> =
            withContext(Dispatchers.IO) {
                try {
                    val response = api.removeFavoriteProduct(ProductRequest(ProductId = productId))
                    if (response.isSuccessful) {
                        Result.success(Unit)
                    } else {
                        Result.failure(Exception("Ошибка удаления из избранного"))
                    }
                } catch (e: Exception) {
                    Result.failure(e)
                }
            }


        suspend fun getCartWithStatus(): Result<List<CartItemApiDto>> = withContext(Dispatchers.IO) {
            try {
                val cartResult = api.getCart()
                if (!cartResult.isSuccessful || cartResult.body() == null) {
                    return@withContext Result.failure<List<CartItemApiDto>>(Exception("Ошибка загрузки корзины"))
                }

                val cart = cartResult.body()!!
                val updatedItems = cart.items.map { item ->
                    // Запрашиваем статус продукта
                    val statusResult = api.getProductStatus(item.productDto._id)

                    if (statusResult.isSuccessful && statusResult.body() != null) {
                        val status = statusResult.body()!!
                        item.copy(
                            productDto = item.productDto.copy(
                                isFavorite = status.inFavorite,
                                inCart = status.inCart,
                                inCompare = status.inCompare
                            )
                        )
                    } else {
                        item
                    }
                }

                Result.success(updatedItems)
            } catch (e: Exception) {
                Result.failure(e)
            }
        }

        suspend fun addToCart(request: ProductRequest): Result<CartApiDto> = withContext(Dispatchers.IO) {
            try {
                val response = api.addToCart(request)
                if (response.isSuccessful && response.body() != null) {
                    Result.success(response.body()!!)
                } else {
                    Result.failure(Exception("Ошибка добавления в корзину"))
                }
            } catch (e: Exception) {
                Result.failure(e)
            }
        }

        suspend fun removeFromCart(request: ProductRequest): Result<CartApiDto> = withContext(Dispatchers.IO) {
            try {
                val response = api.removeFromCart(request)
                if (response.isSuccessful && response.body() != null) {
                    Result.success(response.body()!!)
                } else {
                    Result.failure(Exception("Ошибка удаления из корзины"))
                }
            } catch (e: Exception) {
                Result.failure(e)
            }
        }

        suspend fun changeQuantity(request: ProductRequest): Result<CartApiDto> = withContext(Dispatchers.IO) {
            try {
                val response = api.changeQuantity(request)
                if (response.isSuccessful && response.body() != null) {
                    Result.success(response.body()!!)
                } else {
                    Result.failure(Exception("Ошибка изменения количества"))
                }
            } catch (e: Exception) {
                Result.failure(e)
            }
        }

        suspend fun selectProduct(request: ProductRequest): Result<CartApiDto> = withContext(Dispatchers.IO) {
            try {
                val response = api.selectProduct(request)
                if (response.isSuccessful && response.body() != null) {
                    Result.success(response.body()!!)
                } else {
                    Result.failure(Exception("Ошибка выделения товара"))
                }
            } catch (e: Exception) {
                Result.failure(e)
            }
        }

        suspend fun unselectProduct(request: ProductRequest): Result<CartApiDto> = withContext(Dispatchers.IO) {
            try {
                val response = api.unselectProduct(request)
                if (response.isSuccessful && response.body() != null) {
                    Result.success(response.body()!!)
                } else {
                    Result.failure(Exception("Ошибка снятия выделения"))
                }
            } catch (e: Exception) {
                Result.failure(e)
            }
        }
    }
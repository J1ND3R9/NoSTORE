document.addEventListener('DOMContentLoaded', async () => {
    const products = document.querySelectorAll('.product');
    for (const card of products) {
        const productId = card.dataset.productid;
        const favBtn = card.querySelector('.favorite');
        const cartBtn = card.querySelector('.basket');

        

        try {
            const res = await fetch(`/api/apiproduct/${productId}`);
            const data = await res.json();

            if (data.inFavorite) favBtn.classList.add('active');
            if (data.inCart) cartBtn.classList.add('active');
        } catch (err) {
            console.error('Ошибка загрузки статуса: ', err);
        }

        favBtn.addEventListener('click', async () => {
            const url = favBtn.classList.contains('active') ? '/api/apiproduct/remove_product_favorite' : '/api/apiproduct/add_product_favorite';
            try {
                const response = await fetch(url, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ productId })
                });
                if (!response.ok) throw new Error('Ошибка сервера');
                favBtn.classList.toggle('active');
            } catch (err) {
                alert('Не удалось изменить избранное');
                console.error('Ошибка избранного: ', err);
            }
        });

        cartBtn.addEventListener('click', async () => {
            const url = cartBtn.classList.contains('active') ? '/api/apiproduct/remove_product_cart' : '/api/apiproduct/add_product_cart';
            try {
                const response = await fetch(url, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ productId })
                });
                if (!response.ok) throw new Error('Ошибка сервера');
                cartBtn.classList.toggle('active');
            } catch (err) {
                alert('Не удалось изменить корзину');
                console.error('Ошибка корзины: ', err);
            }
        });
    }
});
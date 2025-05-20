async function fetchUserAndSaveToStorage() {
    try {
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        const response = await fetch('/api/auth/me');
        const data = await response.json();

        if (data.isAuthenticated) {
            localStorage.setItem('user', JSON.stringify(data));
        } else {
            localStorage.removeItem('user');
        }
        return data;
    } catch (error) {
        console.error('Ошибка при получении данных пользователя:', error);
        localStorage.removeItem('user');
        return { isAuthenticated: false };
    }
}
async function circlesQuantities() {
    const quantities = await getQuantities();
    const favBtn = document.querySelector('#favorite-button .icon-wrapper');
    const cartBtn = document.querySelector('#basket-button .icon-wrapper');

    if (quantities.favorite > 0) {
        const badge = document.createElement('div');
        badge.id = 'favBadge'
        badge.className = 'badge';
        badge.innerText = quantities.favorite;
        favBtn.appendChild(badge);
    }
    if (quantities.cart > 0) {
        const badge = document.createElement('div');
        badge.id = 'cartBadge'
        badge.className = 'badge';
        badge.innerText = quantities.cart;
        cartBtn.appendChild(badge);
    }
}

async function getQuantities() {
    const response = await fetch('/api/apiproduct/getQuantities');
    const data = await response.json();

    return {
        favorite: parseInt(data.favoriteQuantity),
        cart: parseInt(data.cartQuantity)
    }
}

function profileModal() {
    const profile = document.getElementById('profile');
    const profileModal = document.getElementById('profile-modal');

    profile.addEventListener('mouseenter', () => {
        profileModal.style.display = 'block';
        setTimeout(() => {
            profileModal.style.opacity = '1';
        }, 10);
    });

    profile.addEventListener('mouseleave', () => {
        setTimeout(() => {
            if (!profileModal.matches(':hover')) {
                profileModal.style.opacity = '0';
                setTimeout(() => {
                    if (!profileModal.matches(':hover')) {
                        profileModal.style.display = 'none';
                    }
                }, 200);
            }
        }, 100);
    });

    profileModal.addEventListener('mouseleave', () => {
        profileModal.style.opacity = '0';
        setTimeout(() => {
            if (!profileModal.matches(':hover')) {
                profileModal.style.display = 'none';
            }
        }, 200);
    });
}

function updateHeaderUI() {
    const userStr = localStorage.getItem('user');
    const user = userStr ? JSON.parse(userStr) : null;
    const profile = document.getElementById('profile-button');
    if (profile && user?.isAuthenticated) {
        profile.addEventListener('click', () => {
            location.href = '/profile/settings';
        });
        if (user.avatar) {
            profile.innerHTML = `
                    <div class="profile-mini_avatar">
                        <img src="/photos/avatars/${user.userId}/${user.avatar}" />
                    </div> 
                    <span id="profile-text">${user.nickname}</span>
                    `
        } else {
            document.getElementById('profile-text').textContent = user.nickname;
        }


    } else {
        profile.innerHTML = `
        <svg width="115" height="125" viewBox="0 0 115 125" fill="none" xmlns="http://www.w3.org/2000/svg">
                        <g id="profile">
                            <path id="p" d="M83.4279 29.4483C82.2718 45.0408 70.4513 57.7608 57.4748 57.7608C44.4982 57.7608 32.6571 45.0438 31.5217 29.4483C30.342 13.2276 41.8439 1.1358 57.4748 1.1358C73.1056 1.1358 84.6076 13.5225 83.4279 29.4483Z" stroke="black" stroke-width="1.49989" stroke-linecap="round" stroke-linejoin="round" />
                            <path id="p_1" d="M57.4858 76.6361C31.8276 76.6361 5.78602 90.7924 0.966996 117.512C0.386 120.733 2.20862 123.824 5.57957 123.824H109.392C112.766 123.824 114.589 120.733 114.008 117.512C109.186 90.7924 83.144 76.6361 57.4858 76.6361Z" stroke="black" stroke-width="1.49989" stroke-miterlimit="10" />
                        </g>
                    </svg>
                    <span id="profile-text">Войти</span>
        `
    }
}

document.addEventListener('DOMContentLoaded', async () => {
    updateHeaderUI();
    const userStr = localStorage.getItem('user');
    const user = userStr ? JSON.parse(userStr) : null;

    if (!user || !user.isAuthenticated) {
        const userData = await fetchUserAndSaveToStorage();
        if (userData.isAuthenticated) {
            updateHeaderUI();
        }
    }

    if (JSON.parse(localStorage.getItem('user'))?.isAuthenticated) {
        profileModal();
    }
    await circlesQuantities();
});
// === Всё, что связано с обновлением верхушки и получением данных о пользователе ===

// === Инициализиации ===

// Инициализация модального окна профиля
function initModal() {
    const profile = document.getElementById('profile');
    const profileModal = document.getElementById('profile-modal');
    let hoverTimeout;

    const showModal = () => {
        clearTimeout(hoverTimeout);
        gsap.killTweensOf(profileModal);

        profileModal.style.display = 'block';
        gsap.to(profileModal, {
            autoAlpha: 1,
            duration: 0.2,
            ease: 'power2.out'
        });
    };

    const hideModal = () => {
        clearTimeout(hoverTimeout);
        hoverTimeout = setTimeout(() => {
            if (!profile.matches(':hover') && !profileModal.matches(':hover')) {
                gsap.to(profileModal, {
                    autoAlpha: 0,
                    duration: 0.2,
                    ease: 'power2.in',
                    onComplete: () => {
                        profileModal.style.display = 'none';
                    }
                });
            }
        }, 100);
    };

    profile.addEventListener('mouseenter', showModal);
    profile.addEventListener('mouseleave', hideModal);

    profileModal.addEventListener('mouseleave', hideModal);
    profileModal.addEventListener('mouseenter', () => {
        clearTimeout(hoverTimeout);
    });

}


// === API функции ===

// Получение юзера из API и сохранение в localStorage
async function fetchUserAndSave(force = false) {
    const cachedUser = localStorage.getItem('user');
    if (cachedUser && !force) {
        return JSON.parse(cachedUser);
    }

    try {
        const response = await fetch('/api/auth/me');
        const data = await response.json();
        if (data.isAuthenticated) {
            localStorage.setItem('user', JSON.stringify(data));
            window.dispatchEvent(new Event("userLoaded"));
        } else {
            localStorage.removeItem('user');
        }
        return data;
    } catch (err) {
        console.error('Ошибка при получении данных пользователя:', error);
        localStorage.removeItem('user');
        return { isAuthenticated: false };
    }
}

// === Вспомогательные функции ===

// Контроллер профиля в header'е.
function profileHeaderHandler(user) {
    const profile = document.getElementById('profile-button');
    if (!profile) return;

    const skeleton = profile.querySelector('.skeleton-loader');
    const guestSvg = profile.querySelector('#profile-guest');
    const guestText = profile.querySelector('#profile-text')
    const userContainer = profile.querySelector('#profile-user-container');

    skeleton.style.display = 'inline-block';
    guestSvg.style.display = 'none';
    guestText.style.display = 'none';
    userContainer.innerHTML = '';
    userContainer.style.display = 'none';

    if (user?.isAuthenticated) {
        if (user.avatar) {
            userContainer.innerHTML = `
                    <div class="profile-mini_avatar" style="background-image: url('/photos/avatars/${user.userId}${user.avatar}');"></div>
                    <span id="profile-text">${user.nickname}</span>
            `;
        } else {
            userContainer.innerHTML = `
                    <div class="profile-mini_avatar" style="background-image: url('/photos/avatars/default.png');"></div>
                    <span id="profile-text">${user.nickname}</span>
            `;
        }

        userContainer.style.display = 'flex';
        userContainer.addEventListener('click', () => {
            location.href = '/profile/settings';
        });

        guestSvg.style.display = 'none';
        guestText.style.display = 'none';
    } else {
        guestSvg.style.display = 'flex';
        guestText.style.display = 'flex';

        guestText.textContent = 'Войти';
    }

    skeleton.style.display = 'none';

    //if (!profile)
    //    return;

    //if (user?.isAuthenticated) {
    //    profile.addEventListener('click', () => {
    //        location.href = '/profile/settings';
    //    });
    //    if (user.avatar) {
    //        profile.innerHTML = `
    //<div class="profile-mini_avatar" style="background-image: url('/photos/avatars/${user.userId}/${user.avatar}');"></div>
    //<span id="profile-text">${user.nickname}</span>
    //`
    //    } else {
    //        document.getElementById('profile-text').textContent = user.nickname;
    //    }
    //} else {
    //    profile.innerHTML = `<svg width="115" height="125" viewBox="0 0 115 125" fill="none" xmlns="http://www.w3.org/2000/svg">
    //                    <g id="profile">
    //                        <path id="p" d="M83.4279 29.4483C82.2718 45.0408 70.4513 57.7608 57.4748 57.7608C44.4982 57.7608 32.6571 45.0438 31.5217 29.4483C30.342 13.2276 41.8439 1.1358 57.4748 1.1358C73.1056 1.1358 84.6076 13.5225 83.4279 29.4483Z" stroke="black" stroke-width="1.49989" stroke-linecap="round" stroke-linejoin="round" />
    //                        <path id="p_1" d="M57.4858 76.6361C31.8276 76.6361 5.78602 90.7924 0.966996 117.512C0.386 120.733 2.20862 123.824 5.57957 123.824H109.392C112.766 123.824 114.589 120.733 114.008 117.512C109.186 90.7924 83.144 76.6361 57.4858 76.6361Z" stroke="black" stroke-width="1.49989" stroke-miterlimit="10" />
    //                    </g>
    //                </svg>
    //                <span id="profile-text">Войти</span>`
    //}

}

// === Экспорт ===
export async function initHeaderUI() {
    const user = await fetchUserAndSave();
    profileHeaderHandler(user);
    if (!JSON.parse(localStorage.getItem('user'))?.isAuthenticated) {
        return;
    }
    initModal();
}

export { fetchUserAndSave };
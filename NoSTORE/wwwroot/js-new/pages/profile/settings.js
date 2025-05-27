document.addEventListener('DOMContentLoaded', () => {
    initSettings();
});

// === Инициализации ===

// Инициализация Settings страницы
function initSettings() {
    const user = localStorage.getItem('user');
    if (!user) {
        alert("Вы не авторизованы.")
        throw new Error("Вы не авторизованы.")
        location.href = "/";
        return;
    }
    setAvatar(user);
    setRegistrationDate();
}

// === API функции ===

// === Вспомогательные функции ===

// Вставка аватарки
function setAvatar(user) {
    const avatarEl = document.getElementById('userAvatar');
    if (!avatarEl) return;

    let pathAvatar = `/photos/avatars/${user.userId}${user.avatar}`;
    if (!user.avatar) {
        pathAvatar = `/photos/avatars/default.png`;
    }
    avatarEl.style.backgroundImage = `url('${pathAvatar}')`
}

// Вставка даты регистрации
function setRegistrationDate() {
    const regEl = document.getElementById('registrationDate');
    const date = new Date(regEl.dataset.time);
    regEl.textContent = date.toLocaleDateString();
}
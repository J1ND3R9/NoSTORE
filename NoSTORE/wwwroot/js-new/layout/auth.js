// === Всё, что связано с аутентификацией пользователя (модалка, логика) ===

// Текущий этап входа (логин, регистер, коды)
let currentStage = 'login';

// Почта regex
const EMAIL_REGEXP = /^(([^<>()[\].,;:\s@"]+(\.[^<>()[\].,;:\s@"]+)*)|(".+"))@(([^<>()[\].,;:\s@"]+\.)+[^<>()[\].,;:\s@"]{2,})$/iu;

// === Инициализации ===

// Инициализация модалки
function initLoginModal() {
    const registerScn = document.getElementById('register-section');
    const codeScn = document.getElementById('code-section');

    const noAccountBtn = document.getElementById('no-account');
    const submitBtn = document.getElementById('check-combination');
    const profileBtn = document.getElementById('profile-button');

    const emailInput = document.getElementById('email');

    const loginModal = document.getElementById('login-modal');

    emailInput.addEventListener('input', () => inputEmailValid(emailInput));

    loginModal.addEventListener('click', (e) => loginModalVisibilityController(loginModal, e));
    noAccountBtn.addEventListener('click', () => registerSectionVisibility(noAccountBtn, submitBtn, registerScn));
    submitBtn.addEventListener('click', (e) => initSubmit(e, noAccountBtn, submitBtn, codeScn))
    profileBtn.addEventListener('click', () => showWindow(loginModal));

    document.addEventListener('keydown', (e) => loginModalHotkey(loginModal, e));

    // Временное решение?
    const logoutBtn = document.getElementById('button-logout');
    logoutBtn.addEventListener('click', () => logout())
}

// Инициализация события подтверждения данных
function initSubmit(e, noAccBtn, subBtn, codeScn) {
    e.preventDefault();
    const emailInput = document.getElementById('email');
    const email = emailInput.value;
    if (!email) {
        throw new Error('Не найдена почта');
        return;
    }
    if (!isEmailValid(emailInput.value)) {
        alert('Неверно введена почта');
        emailInput.focus();
        return;
    }
    switch (currentStage) {
        case 'register': {
            registerStageCheck(email, noAccBtn, subBtn, codeScn);
            break;
        }
        case 'login': {
            loginStageCheck(email, noAccBtn, subBtn, codeScn);
            break;
        }
        case 'codeRegister': {
            registerCodeStageCheck(email);
            break;
        }
        case 'codeLogin': {
            loginCodeStageCheck(email);
            break;
        }
        default: {
            throw new Error('Неизвестный этап');
            break;
        }
    }
}


// === API функции ===

// Контроллер ответа от сервера
async function handleResponse(response) {
    if (!response.ok) {
        let message = 'Ошибка со стороны сервера';
        try {
            const data = await response.json();
            if (data.error) message = data.error;
        } catch (_) { }
        throw new Error(message);
    }
}

// Выход из профиля
async function logout() {
    try {
        const response = await fetch('/api/auth/logout', {
            method: 'POST'
        });
        await handleResponse(response);
        localStorage.removeItem('user');
        location.reload();
    } catch (err) {
        console.error(err.message);
    }
}

// Сохранение пользователя в localStorage
async function fetchUserAndSaveToStorage() {
    try {
        const response = await fetch('/api/auth/me');
        const data = await response.json();

        await handleResponse(response);

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

// Показ окна при входе
async function showWindow(modal) {
    try {
        const response = await fetch('/api/auth/me');
        await handleResponse(response);
        const data = await response.json();
        if (!data.isAuthenticated) {
            modal.style.display = 'flex';
            setTimeout(() => {
                modal.classList.add('active');
            }, 10);
        }
    } catch (err) {
        console.error(err.message);
    }
}

// Этап входа
async function loginStageCheck(email, noAccBtn, subBtn, codeScn) {
    try {
        const password = document.getElementById('password').value;
        const response = await fetch('/api/auth/login/try-send-code', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password })
        });
        await handleResponse(response);
        stageComplete('codeLogin', noAccBtn, subBtn, codeScn);
    } catch (err) {
        console.error(err.message);
    }
}

// Этап входа с кодом
async function loginCodeStageCheck(email) {
    try {
        const password = document.getElementById('password').value;
        const code = document.getElementById('code').value;

        const response = await fetch('/api/auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password, code })
        });
        await handleResponse(response);
        location.reload();
    } catch (err) {
        console.error(err.message);
    }
}

// Этап регистрации
async function registerStageCheck(email, noAccBtn, subBtn, codeScn) {
    try {
        const response = await fetch('/api/auth/register/try-send-code', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email })
        });
        await handleResponse(response);
        stageComplete('codeRegister', noAccBtn, subBtn, codeScn);
    } catch (err) {
        console.error(err.message);
    }

}

// Этап регистрации с кодом
async function registerCodeStageCheck(email) {
    try {
        const password = document.getElementById('password').value;
        const nickname = document.getElementById('nickname').value;
        const code = document.getElementById('code').value;

        const response = await fetch('/api/auth/register', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password, nickname, code })
        });
        await handleResponse(response);
        location.reload();
    } catch (err) {
        console.error(err.message);
    }
}

// === Вспомогательные функции ===

// Контроллер управления видимостью модалки
function loginModalVisibilityController(modal, e) {
    if (modal.style.display === 'flex' && e.target === modal) {
        modal.classList.remove('active');
        setTimeout(() => {
            modal.style.display = 'none'
        }, 200);
    }
}

// Инициализация хоткея для закрытия окна
function loginModalHotkey(modal, e) {
    if (modal.style.display === 'flex' && e.key === 'Escape') {
        modal.classList.remove('active');
        setTimeout(() => {
            modal.style.display = 'none'
        }, 200);
    }
}

// Проверка на корректность почты при вводе
function inputEmailValid(input) {
    if (!isEmailValid(input.value)) {
        input.style.borderColor = '#E36B3F'; // variables.$bad-color;
    } else {
        input.style.borderColor = '#66A7C8'; // variables.$additional-color;
    }
}

// Отключения кнопок при прохождении этапов
function stageComplete(stage, noAccBtn, subBtn, codeScn) {
    noAccBtn.disabled = true;
    subBtn.textContent = 'Войти';
    currentStage = stage;
    codeScn.style.display = 'flex';
    setTimeout(() => {
        codeScn.classList.add('active');
    }, 10);
}

// Управление видимостью секции регистрации
function registerSectionVisibility(noAccBtn, subBtn, regScn) {
    subBtn.textContent = 'Продолжить';
    if (currentStage != 'register') {
        currentStage = 'register';
        noAccBtn.textContent = 'Есть аккаунт';
        regScn.style.display = 'flex';
        setTimeout(() => {
            regScn.classList.add('active');
        }, 10);
    } else {
        currentStage = 'login';
        noAccBtn.textContent = 'Нет аккаунта';
        regScn.classList.remove('active');
        setTimeout(() => {
            regScn.style.display = 'none';
        }, 200);
    }
}

// === Вспомогательные утилиты ===

// Проверка валидности почты
function isEmailValid(value) {
    return EMAIL_REGEXP.test(value);
}

// === Экспорт ===
export function initAuthModal() {
    initLoginModal();
    fetchUserAndSaveToStorage();
}
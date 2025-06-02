import { fetchUserAndSave } from '../../layout/headerUI.js'

// Почта regex
const EMAIL_REGEXP = /^(([^<>()[\].,;:\s@"]+(\.[^<>()[\].,;:\s@"]+)*)|(".+"))@(([^<>()[\].,;:\s@"]+\.)+[^<>()[\].,;:\s@"]{2,})$/iu;

// Пароль regex
const PASSWORD_REGEXP = /^(?=.*[A-Za-z])(?=.*\d).{6,}$/;

// === Инициализации ===

// Инициализация Settings страницы
async function initSettings() {
    const nicknameBtn = document.getElementById('changeNickname');
    const emailBtn = document.getElementById('changeEmail');
    const phoneBtn = document.getElementById('changePhone');
    const passwordBtn = document.getElementById('changePassword');
    const deleteProfileBtn = document.getElementById('deleteProfile');
    const avatarBtn = document.getElementById('userAvatar');
    const avatarInp = document.getElementById('avatarInput');

    nicknameBtn.addEventListener('click', () => openModal('nickname'))
    emailBtn.addEventListener('click', () => openModal('email'))
    phoneBtn.addEventListener('click', () => openModal('phone'))
    passwordBtn.addEventListener('click', () => openModal('password'))
    deleteProfileBtn.addEventListener('click', () => openModal('delete'))

    const user = JSON.parse(localStorage.getItem('user'));
    if (!user) {
        alert("Вы не авторизованы.")
        throw new Error("Вы не авторизованы.")
        location.href = "/";
        return;
    }

    avatarBtn.addEventListener('click', () => {
        avatarInp.click();
    });
    avatarInp.addEventListener('change', async (e) => changeAvatar(e, user));
    setAvatar(user);
    modalVisibilityController();
    setRegistrationDate();

}

// === API функции ===

// Смена аватарки
async function changeAvatar(e) {
    const file = e.target.files[0];
    if (!file) return;

    if (!file.type.startsWith('image/')) {
        alert("Пожалуйта, выберите изображение!");
        return;
    }

    const formData = new FormData();
    formData.append('avatar', file);

    const response = await fetch('/api/auth/change/avatar', {
        method: 'POST',
        body: formData
    });

    if (response.ok) {
        await fetchUserAndSave(true);
        location.reload();
    } else {
        alert("Ошибка загрузки данных");
    }
}

// === Вспомогательные функции ===

// Клик вне модального окна
function modalVisibilityController() {
    let isMouseDownOnOverlay = false;

    const modal = document.getElementById('settings-modal');

    modal.addEventListener('mousedown', (e) => {
        if (e.target === modal) {
            isMouseDownOnOverlay = true;
        }
    });

    modal.addEventListener('mouseup', (e) => {
        if (isMouseDownOnOverlay && e.target === modal) {
            modal.classList.remove('active');
            setTimeout(() => {
                modal.style.display = 'none';
            }, 200);
        }
        isMouseDownOnOverlay = false;
    });
    document.addEventListener('keydown', (e) => {
        if (modal.style.display === 'flex' && e.key === 'Escape') {
            modal.classList.remove('active');
            setTimeout(() => {
                modal.style.display = 'none'
            }, 200);
        }
    })
}

// Показ кода для шагов
function displayCode(element) {
    const submit = document.getElementById('submitBtn');
    submit.textContent = 'Сохранить';
    const desc = document.getElementById('modal-description');
    desc.textContent = 'Введите код, который мы отправили вам на почту.'
    const newInput = document.createElement('input');
    newInput.type = 'text';
    newInput.name = 'code';
    newInput.placeholder = 'Введите код';
    newInput.required = true;
    newInput.style.overflow = 'hidden';
    newInput.style.maxHeight = '0px';
    newInput.style.opacity = '0';
    element.insertAdjacentElement('afterend', newInput)

    gsap.set(newInput, { y: -20 })

    gsap.to(newInput, {
        maxHeight: 500,
        y: 0,
        autoAlpha: 1,
        duration: 0.5
    });

    gsap.set(desc, { y: -20 })
    gsap.to(desc, {
        maxHeight: 500,
        autoAlpha: 1,
        y: 0,
        duration: 0.5
    });
}

function openModal(type) {
    const modal = document.getElementById('settings-modal');
    const modalTitle = document.getElementById('modal-title');
    const modalForm = document.getElementById('modal-form');

    modalForm.onsubmit = null;
    modal.style.display = 'flex';

    setTimeout(() => {
        modal.classList.add('active');
    }, 10);

    switch (type) {
        case 'nickname':
            stepNicknameChange(modalTitle, modalForm);
            break;
        case 'email':
            stepEmailChange(modalTitle, modalForm);
            break;
        case 'phone':
            modalTitle.textContent = 'Телефон';
            modalForm.innerHTML = `
                <input type="text" placeholder="Новый телефон" required />
                <button type="submit">Сохранить</button>
            `
            break;
        case 'password':
            stepPasswordChange(modalTitle, modalForm);
            break;
        case 'delete':
            break;
    }
}
// Шаг - отправка кода на почту (смена пароля)
function stepPasswordChange(modalTitle, modalForm) {
    modalTitle.textContent = 'Смена пароля';
    modalForm.innerHTML = `
                <input type="password" name="oldPass" placeholder="Старый пароль" required />
                <input type="password" name="newPass" placeholder="Новый пароль" required />
                <button type="submit" id="submitBtn">Продолжить</button>
            `
    modalForm.onsubmit = async (e) => {
        e.preventDefault();
        const oldPass = modalForm.oldPass.value;
        const newPass = modalForm.newPass.value;

        try {
            if (!isPasswordValid(newPass)) throw new Error("Новый пароль не прошёл валидность!")
            if (oldPass === newPass) throw new Error("Пароли совпадают.")

            const responseExist = await fetch('/api/auth/change/check-password', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ oldPassword: oldPass, newPassword: newPass })
            });
            if (!responseExist.ok) throw new Error("Что то не так с паролем")
            stepPasswordConfirm(modalForm);
        } catch (err) {
            alert(err.message);
        }
    }
}

// Шаг - подтверждение кода (смена пароля)
async function stepPasswordConfirm(modalForm) {
    const emailInput = document.querySelector('input[name="newPass"]');

    displayCode(emailInput);

    try {
        const response_code = await fetch('/api/auth/change/send-code', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        });
        if (!response_code.ok) throw new Error("Не удалось отправить код.")
    } catch (err) {
        alert(err.message)
    }

    modalForm.onsubmit = async (e) => {
        e.preventDefault();
        const oldPass = modalForm.oldPass.value;
        const newPass = modalForm.newPass.value;

        if (!isPasswordValid(newPass)) throw new Error("Новый пароль не прошёл валидность!")
        if (oldPass === newPass) throw new Error("Пароли совпадают.")

        const code = modalForm.code.value;

        try {
            const responseExist = await fetch('/api/auth/change/check-password', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ oldPassword: oldPass, newPassword: newPass })
            });
            if (!responseExist.ok) throw new Error("Что то не так с паролем")

            const response = await fetch('/api/auth/change/password', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ oldPassword: oldPass, newPassword: newPass, code: code })
            });
            if (!response.ok) throw new Error("Вы ввели неверный код")
            location.reload();
        } catch (err) {
            alert(err.message);
        }
    }
}


// Шаг - смена никнейма
function stepNicknameChange(modalTitle, modalForm) {
    modalTitle.textContent = 'Смена никнейма';
    modalForm.innerHTML = `
                <input type="text" name="nick" placeholder="Новый никнейм" required />
                <button type="submit">Сохранить</button>
            `
    modalForm.onsubmit = async (e) => {
        e.preventDefault();
        const nickname = modalForm.nick.value;

        if (!nickname) return;

        try {
            const response = await fetch('/api/auth/change/nickname', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ nickname: nickname })
            });
            if (!response.ok) throw new Error("Не удалось сменить никнейм")
            location.reload();
        } catch (err) {
            alert(err.message);
        }
    }
}

// Шаг - отправка кода на почту (смена почты)
function stepEmailChange(modalTitle, modalForm) {
    modalTitle.textContent = 'Смена почты';
    modalForm.innerHTML = `
                <input type="text" name="email" placeholder="Новая почта" required />
                <button type="submit" id="submitBtn">Продолжить</button>
            `;
    modalForm.onsubmit = async (e) => {
        e.preventDefault();
        const email = modalForm.email.value;

        try {
            if (!isEmailValid(email)) throw new Error("Почта не прошла ваидность!")
            const responseExist = await fetch('/api/auth/change/email_exist', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email: email })
            });
            if (!responseExist.ok) throw new Error("Такая почта уже существует.")
            stepEmailConfirm(modalForm);
        } catch (err) {
            alert(err.message);
        }
    }
}

// Шаг - подтверждение кода (смена почты)
async function stepEmailConfirm(modalForm) {
    const emailInput = document.querySelector('input[name="email"]');

    const submit = document.getElementById('submitBtn');
    submit.textContent = 'Сохранить';

    const newInput = document.createElement('input');
    newInput.type = 'text';
    newInput.name = 'code';
    newInput.placeholder = 'Введите код';
    newInput.required = true;
    newInput.style.overflow = 'hidden';
    newInput.style.maxHeight = '0px';
    newInput.style.opacity = '0';

    const desc = document.getElementById('modal-description');
    desc.textContent = 'Введите код, который мы отправили вам на почту.'
    gsap.set(desc, { y: -20 })
    gsap.to(desc, {
        maxHeight: 500,
        autoAlpha: 1,
        y: 0,
        duration: 0.5
    });

    emailInput.insertAdjacentElement('afterend', newInput)

    gsap.set(newInput, { y: -20 })

    gsap.to(newInput, {
        maxHeight: 500,
        y: 0,
        autoAlpha: 1,
        duration: 0.5
    });

    try {
        const response_code = await fetch('/api/auth/change/send-code', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        });
        if (!response_code.ok) throw new Error("Не удалось отправить код.")
    } catch (err) {
        alert(err.message)
    }

    modalForm.onsubmit = async (e) => {
        e.preventDefault();
        const email = modalForm.email.value;
        const code = modalForm.code.value;

        if (!email) return;

        try {
            const response = await fetch('/api/auth/change/email', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email: email, code: code })
            });
            if (!response.ok) throw new Error("Вы ввели неверный код")
            location.reload();
        } catch (err) {
            alert(err.message);
        }
    }
}

// Вставка аватарки
function setAvatar(user) {
    const avatarEl = document.getElementById('userAvatar');
    if (!avatarEl) return;

    let pathAvatar;

    if (!user || !user.avatar) {
        pathAvatar = `/photos/avatars/default.png`;
    } else {
        pathAvatar = `/photos/avatars/${user.userId}${user.avatar}`;
    }

    const cacheBuster = `?v=${Date.now()}`;
    avatarEl.style.backgroundImage = `url('${pathAvatar}${cacheBuster}')`
}

// Вставка даты регистрации
function setRegistrationDate() {
    const regEl = document.getElementById('registrationDate');
    const date = new Date(regEl.dataset.time);
    regEl.textContent = date.toLocaleDateString();
}

// === Вспомогательные утилиты ===

// Проверка валидности почты
function isEmailValid(value) {
    return EMAIL_REGEXP.test(value);
}

// Проверка валидности пароля
function isPasswordValid(value) {
    return PASSWORD_REGEXP.test(value);
}

export { initSettings };
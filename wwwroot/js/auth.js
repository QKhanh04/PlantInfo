let currentMode = 'login';

function switchToLogin() {
    if (currentMode === 'login') return;
    currentMode = 'login';

    document.querySelectorAll('.tab-button').forEach((btn, index) => {
        btn.classList.toggle('active', index === 0);
    });

    document.querySelector('.tab-indicator').classList.remove('register');

    document.getElementById('loginForm').classList.add('active');
    document.getElementById('loginForm').classList.remove('inactive');

    document.getElementById('registerForm').classList.add('inactive');
    document.getElementById('registerForm').classList.remove('active');
}

function switchToRegister() {
    if (currentMode === 'register') return;
    currentMode = 'register';

    document.querySelectorAll('.tab-button').forEach((btn, index) => {
        btn.classList.toggle('active', index === 1);
    });

    document.querySelector('.tab-indicator').classList.add('register');

    document.getElementById('registerForm').classList.add('active');
    document.getElementById('registerForm').classList.remove('inactive');

    document.getElementById('loginForm').classList.add('inactive');
    document.getElementById('loginForm').classList.remove('active');
}



function togglePassword(inputId, button) {
    const input = document.getElementById(inputId);
    const icon = button.querySelector('i');

    if (input.type === 'password') {
        input.type = 'text';
        icon.classList.remove('bi-eye');
        icon.classList.add('bi-eye-slash'); // icon khi đang hiển thị mật khẩu
    } else {
        input.type = 'password';
        icon.classList.remove('bi-eye-slash');
        icon.classList.add('bi-eye'); // icon khi đang ẩn mật khẩu
    }
}


window.initNavMenu = () => {
    const toggle = document.querySelector(".nav__toggle");
    const menu = document.querySelector(".nav__menu");

    if (!toggle || !menu) return;

    // Abre/fecha ao clicar no botão
    toggle.addEventListener("click", (e) => {
        e.stopPropagation(); // impede fechar imediatamente
        menu.classList.toggle("open");
    });

    // Fecha ao clicar em qualquer item do menu
    menu.querySelectorAll(".nav__link").forEach(link => {
        link.addEventListener("click", () => {
            menu.classList.remove("open");
        });
    });

    // Fecha ao clicar fora do menu
    document.addEventListener("click", (e) => {
        const clickedInsideMenu = menu.contains(e.target);
        const clickedToggle = toggle.contains(e.target);

        if (!clickedInsideMenu && !clickedToggle) {
            menu.classList.remove("open");
        }
    });
};

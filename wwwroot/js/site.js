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

window.pdfInterop = {
    downloadReport: async function (reportId) {
        const reportElement = document.querySelector(".report");

        if (!reportElement) {
            throw new Error("Elemento do rapportino não encontrado para exportação PDF.");
        }

        if (typeof window.html2pdf === "undefined") {
            throw new Error("Biblioteca html2pdf não disponível.");
        }

        const filename = reportId ? `rapportino_${reportId}.pdf` : "rapportino.pdf";

        await window.html2pdf()
            .set({
                margin: [0.2, 0.2, 0.2, 0.2],
                filename,
                image: { type: "jpeg", quality: 0.98 },
                html2canvas: { scale: 2, logging: false },
                jsPDF: { unit: "in", format: "a4", orientation: "portrait" }
            })
            .from(reportElement)
            .save();
    }
};

let clientPad = null;
let techPad = null;
let resizeHandlerRegistered = false;

document.addEventListener("DOMContentLoaded", () => {
    window.signaturePadInterop.init();
});

window.addEventListener("load", () => {
    window.signaturePadInterop.init();
});

/* ============================================================
   RESIZE CANVAS — SEGURO, NÃO APAGA ASSINATURA
============================================================ */
function resizeCanvas(canvas, pad) {
    if (!canvas) return;

    if (canvas.offsetWidth < 10 || canvas.offsetHeight < 10) {
        return;
    }

    const ratio = Math.max(window.devicePixelRatio || 1, 1);

    let data = null;
    if (pad && typeof pad.toData === "function" && !pad.isEmpty()) {
        try {
            data = pad.toData();
        } catch (e) {
            console.error("Erro ao salvar dados da assinatura:", e);
        }
    }

    canvas.width = canvas.offsetWidth * ratio;
    canvas.height = canvas.offsetHeight * ratio;

    const context = canvas.getContext("2d");
    context.setTransform(ratio, 0, 0, ratio, 0, 0);

    if (pad && data) {
        try {
            pad.fromData(data);
        } catch (e) {
            console.error("Erro ao restaurar assinatura:", e);
        }
    }
}

/* ============================================================
   SIGNATURE PAD INTEROP — SEGURO E ESTÁVEL
============================================================ */
window.signaturePadInterop = {
    init: function () {
        if (typeof SignaturePad === "undefined") {
            console.error("SignaturePad non è stato caricato.");
            return;
        }

        const clientCanvas = document.getElementById("signature-pad");
        const techCanvas = document.getElementById("signature-tech");

        if (clientCanvas && !clientPad) {
            try {
                clientPad = new SignaturePad(clientCanvas, {
                    backgroundColor: "rgb(255, 255, 255)",
                    penColor: "rgb(0, 0, 0)"
                });
                resizeCanvas(clientCanvas, clientPad);
            } catch (e) {
                console.error("Erro ao inicializzare clientPad:", e);
                clientPad = null;
            }
        }

        if (techCanvas && !techPad) {
            try {
                techPad = new SignaturePad(techCanvas, {
                    backgroundColor: "rgb(255, 255, 255)",
                    penColor: "rgb(0, 0, 0)"
                });
                resizeCanvas(techCanvas, techPad);
            } catch (e) {
                console.error("Erro ao inicializzare techPad:", e);
                techPad = null;
            }
        }

        if (!resizeHandlerRegistered) {
            window.addEventListener("resize", () => {
                const c1 = document.getElementById("signature-pad");
                const c2 = document.getElementById("signature-tech");

                if (c1) resizeCanvas(c1, clientPad);
                if (c2) resizeCanvas(c2, techPad);
            });

            resizeHandlerRegistered = true;
        }
    },

    clearClient: () => {
        if (clientPad && typeof clientPad.clear === "function") {
            try {
                clientPad.clear();
            } catch (e) {
                console.error("Erro ao limpar clientPad:", e);
            }
        }
    },

    clearTech: () => {
        if (techPad && typeof techPad.clear === "function") {
            try {
                techPad.clear();
            } catch (e) {
                console.error("Erro ao limpar techPad:", e);
            }
        }
    },

    getClientSignature: () => {
        if (clientPad &&
            typeof clientPad.isEmpty === "function" &&
            typeof clientPad.toDataURL === "function" &&
            !clientPad.isEmpty()) {

            try {
                return clientPad.toDataURL("image/png");
            } catch (e) {
                console.error("Erro ao gerar assinatura cliente:", e);
                return "";
            }
        }
        return "";
    },

    getTechSignature: () => {
        if (techPad &&
            typeof techPad.isEmpty === "function" &&
            typeof techPad.toDataURL === "function" &&
            !techPad.isEmpty()) {

            try {
                return techPad.toDataURL("image/png");
            } catch (e) {
                console.error("Erro ao gerar assinatura tecnico:", e);
                return "";
            }
        }
        return "";
    }
};

/* ============================================================
   REINICIALIZA PAD SE O BLAZOR QUEBRAR O OBJETO
============================================================ */
setInterval(() => {
    const clientCanvas = document.getElementById("signature-pad");
    const techCanvas = document.getElementById("signature-tech");

    if (clientCanvas && !clientPad) {
        console.warn("clientPad estava null — reinicializando automaticamente.");
        window.signaturePadInterop.init();
    }

    if (techCanvas && !techPad) {
        console.warn("techPad estava null — reinicializando automaticamente.");
        window.signaturePadInterop.init();
    }
}, 500);

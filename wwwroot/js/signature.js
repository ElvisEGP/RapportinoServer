let clientPad = null;
let techPad = null;
let resizeHandlerRegistered = false;

function resizeCanvas(canvas, pad) {
    if (!canvas) return;

    // 🔥 Evita reset quando o canvas ainda não tem tamanho real
    if (canvas.offsetWidth < 10 || canvas.offsetHeight < 10) {
        return;
    }

    const ratio = Math.max(window.devicePixelRatio || 1, 1);

    // Salva o desenho atual
    const data = pad && !pad.isEmpty() ? pad.toData() : null;

    canvas.width = canvas.offsetWidth * ratio;
    canvas.height = canvas.offsetHeight * ratio;

    const context = canvas.getContext("2d");
    context.setTransform(ratio, 0, 0, ratio, 0, 0);

    // 🔥 Só restaura se havia conteúdo
    if (pad && data) {
        pad.fromData(data);
    }
}


window.signaturePadInterop = {
    init: function () {
        if (typeof SignaturePad === "undefined") {
            console.error("SignaturePad non è stato caricato.");
            return;
        }

        const clientCanvas = document.getElementById("signature-pad");
        const techCanvas = document.getElementById("signature-tech");

        if (clientCanvas) {
            clientPad = new SignaturePad(clientCanvas, {
                backgroundColor: "rgb(255, 255, 255)",
                penColor: "rgb(0, 0, 0)"
            });
            resizeCanvas(clientCanvas, clientPad);
        }

        if (techCanvas) {
            techPad = new SignaturePad(techCanvas, {
                backgroundColor: "rgb(255, 255, 255)",
                penColor: "rgb(0, 0, 0)"
            });
            resizeCanvas(techCanvas, techPad);
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

    clearClient: () => clientPad?.clear(),
    clearTech: () => techPad?.clear(),

    getClientSignature: () => {
        if (clientPad && !clientPad.isEmpty()) {
            return clientPad.toDataURL("image/png");
        }
        return "";
    },

    getTechSignature: () => {
        if (techPad && !techPad.isEmpty()) {
            return techPad.toDataURL("image/png");
        }
        return "";
    }
};

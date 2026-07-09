window.signaturePadInterop = (function () {
    const pads = {};

    function resizeCanvas(canvas, pad) {
        if (!canvas) return;

        const width = canvas.offsetWidth;
        const height = canvas.offsetHeight;

        if (width < 10 || height < 10) return;

        const ratio = Math.max(window.devicePixelRatio || 1, 1);

        let data = null;
        if (pad && typeof pad.toData === "function" && !pad.isEmpty()) {
            try {
                data = pad.toData();
            } catch (e) {
                console.error("Erro ao salvar assinatura antes do resize:", e);
            }
        }

        canvas.width = width * ratio;
        canvas.height = height * ratio;

        const context = canvas.getContext("2d");
        if (context) {
            context.setTransform(ratio, 0, 0, ratio, 0, 0);
        }

        if (pad && data) {
            try {
                pad.fromData(data);
            } catch (e) {
                console.error("Erro ao restaurar assinatura após resize:", e);
            }
        }
    }

    return {
        init: function (canvasId) {
            if (typeof SignaturePad === "undefined") {
                console.error("SignaturePad não foi carregado.");
                return;
            }

            const canvas = document.getElementById(canvasId);
            if (!canvas) {
                console.error("Canvas não encontrado:", canvasId);
                return;
            }

            if (pads[canvasId]) {
                return;
            }

            try {
                const pad = new SignaturePad(canvas, {
                    backgroundColor: "rgb(255, 255, 255)",
                    penColor: "rgb(0, 0, 0)"
                });

                pads[canvasId] = pad;
                resizeCanvas(canvas, pad);

                window.addEventListener("resize", () => {
                    const c = document.getElementById(canvasId);
                    if (c && pads[canvasId]) {
                        resizeCanvas(c, pads[canvasId]);
                    }
                });
            } catch (e) {
                console.error("Erro ao inicializar SignaturePad:", e);
            }
        },

        clear: function (canvasId) {
            const pad = pads[canvasId];
            if (pad && typeof pad.clear === "function") {
                try {
                    pad.clear();
                } catch (e) {
                    console.error("Erro ao limpar assinatura:", e);
                }
            }
        },

        getSignature: function (canvasId) {
            const pad = pads[canvasId];
            if (pad &&
                typeof pad.isEmpty === "function" &&
                typeof pad.toDataURL === "function" &&
                !pad.isEmpty()) {
                try {
                    return pad.toDataURL("image/png");
                } catch (e) {
                    console.error("Erro ao gerar assinatura:", e);
                    return "";
                }
            }

            return "";
        },

        reset: function (canvasId) {
            delete pads[canvasId];
        }
    };
})();

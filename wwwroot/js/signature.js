let clientPad = null;
let techPad = null;
let resizeHandlerRegistered = false;

function resizeCanvas(canvas, pad) {
    if (!canvas) {
        return;
    }

    const ratio = Math.max(window.devicePixelRatio || 1, 1);
    const data = pad && !pad.isEmpty() ? pad.toData() : null;

    canvas.width = canvas.offsetWidth * ratio;
    canvas.height = canvas.offsetHeight * ratio;

    const context = canvas.getContext("2d");
    context.setTransform(ratio, 0, 0, ratio, 0, 0);

    if (pad) {
        pad.clear();

        if (data) {
            pad.fromData(data);
        }
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
            window.addEventListener("resize", function () {
                const currentClientCanvas = document.getElementById("signature-pad");
                const currentTechCanvas = document.getElementById("signature-tech");

                if (currentClientCanvas) {
                    resizeCanvas(currentClientCanvas, clientPad);
                }

                if (currentTechCanvas) {
                    resizeCanvas(currentTechCanvas, techPad);
                }
            });

            resizeHandlerRegistered = true;
        }
    },

    clearClient: function () {
        if (clientPad) {
            clientPad.clear();
        }
    },

    clearTech: function () {
        if (techPad) {
            techPad.clear();
        }
    },

    getClientSignature: function () {
        if (clientPad && !clientPad.isEmpty()) {
            return clientPad.toDataURL("image/png");
        }

        return "";
    },

    getTechSignature: function () {
        if (techPad && !techPad.isEmpty()) {
            return techPad.toDataURL("image/png");
        }

        return "";
    }
};

window.pdfInterop = {
    downloadReport: function (reportId) {
        if (typeof html2pdf === "undefined") {
            console.error("html2pdf non è stato caricato.");
            return;
        }

        const element = document.querySelector('.report-container');
        if (!element) {
            console.error("Elemento .report-container non trovato.");
            return;
        }

        const opt = {
            margin: 10,
            filename: `Rapportino_${reportId}.pdf`,
            image: { type: 'jpeg', quality: 0.98 },
            html2canvas: { scale: 2, useCORS: true },
            jsPDF: { unit: 'mm', format: 'a4', orientation: 'portrait' }
        };

        html2pdf().from(element).set(opt).save();
    }
};
/* wwwroot/assets/js/rotate.js */

const ROTATION_SPEED = 0.0005;
const LAYER_SPACING = 0.5;
const LAYER_COUNT = 15;
const MIN_SCALE = 0.005;
const SVG_NS = "http://www.w3.org/2000/svg";
const states = new WeakMap();

export async function init3DLogo(root) {
    const container = resolveRoot(root);
    if (!container || states.has(container)) {
        return;
    }

    const spinningGroup = container.querySelector(".spinning-group");
    const canvas = container.querySelector(".canvas");
    if (!spinningGroup || !canvas) {
        return;
    }

    const state = {
        animationId: 0,
        frontViewContainer: null,
        backViewContainer: null,
        layers: []
    };

    states.set(container, state);

    try {
        const logoUrl = container.dataset.logoUrl || "Logo/LOGO.svg";
        const response = await fetch(new URL(logoUrl, document.baseURI));
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        const svgText = await response.text();
        const parser = new DOMParser();
        const svgDoc = parser.parseFromString(svgText, "image/svg+xml");
        const svgEl = svgDoc.documentElement;
        const parserError = svgDoc.querySelector("parsererror");
        if (!svgEl || parserError) {
            throw new Error("Invalid SVG logo.");
        }

        spinningGroup.replaceChildren();
        [...canvas.querySelectorAll("defs, style")].forEach(node => node.remove());

        let viewBox = [0, 0, 100, 100];
        const viewBoxText = svgEl.getAttribute("viewBox");
        if (viewBoxText) {
            viewBox = viewBoxText.split(/[\s,]+/).map(Number);
            canvas.setAttribute("viewBox", viewBoxText);
        } else {
            const width = Number.parseFloat(svgEl.getAttribute("width")) || 100;
            const height = Number.parseFloat(svgEl.getAttribute("height")) || 100;
            viewBox = [0, 0, width, height];
            canvas.setAttribute("viewBox", `0 0 ${width} ${height}`);
        }

        const centerX = viewBox[0] + (viewBox[2] / 2);
        const centerY = viewBox[1] + (viewBox[3] / 2);
        const centerOriginCss = `${centerX}px ${centerY}px`;

        svgEl.querySelectorAll("defs, style").forEach(node => {
            canvas.insertBefore(node.cloneNode(true), canvas.firstChild);
        });

        state.backViewContainer = document.createElementNS(SVG_NS, "g");
        state.frontViewContainer = document.createElementNS(SVG_NS, "g");
        spinningGroup.append(state.backViewContainer, state.frontViewContainer);

        const visualNodes = [...svgEl.children].filter(node =>
            node.nodeType === Node.ELEMENT_NODE
            && !["defs", "style", "metadata", "title", "desc"].includes(node.tagName.toLowerCase()));

        visualNodes.forEach(originalNode => {
            for (let index = 0; index < LAYER_COUNT; index++) {
                const clone = originalNode.cloneNode(true);
                clone.style.transformOrigin = centerOriginCss;
                clone.style.transformBox = "view-box";

                if (index > 0 && index < LAYER_COUNT - 1) {
                    clone.style.filter = "brightness(0.85)";
                }

                state.layers.push({
                    element: clone,
                    z: (index - LAYER_COUNT / 2) * LAYER_SPACING
                });
                state.backViewContainer.appendChild(clone);
            }
        });

        if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
            renderStatic(state);
            return;
        }

        state.animationId = requestAnimationFrame(time => animate(container, time));
    } catch (error) {
        states.delete(container);
        console.error("Error loading 3D logo:", error);
    }
}

export function dispose3DLogo(root) {
    const container = resolveRoot(root);
    const state = container ? states.get(container) : null;
    if (!state) {
        return;
    }

    if (state.animationId) {
        cancelAnimationFrame(state.animationId);
    }

    states.delete(container);
}

function animate(container, time) {
    const state = states.get(container);
    if (!state?.frontViewContainer || !state.backViewContainer) {
        return;
    }

    const angle = time * ROTATION_SPEED;
    const cosAngle = Math.cos(angle);
    const sinAngle = Math.sin(angle);
    const isFrontView = cosAngle > 0;

    state.frontViewContainer.style.visibility = isFrontView ? "visible" : "hidden";
    state.backViewContainer.style.visibility = isFrontView ? "hidden" : "visible";

    state.layers.forEach(layer => {
        const x = layer.z * sinAngle;
        const scaleX = Math.max(Math.abs(cosAngle), MIN_SCALE);
        const direction = isFrontView ? 1 : -1;

        layer.element.setAttribute("transform", `translate(${x}, 0) scale(${scaleX * direction}, 1)`);

        if (isFrontView && layer.element.parentElement !== state.frontViewContainer) {
            state.frontViewContainer.appendChild(layer.element);
        } else if (!isFrontView && layer.element.parentElement !== state.backViewContainer) {
            state.backViewContainer.appendChild(layer.element);
        }
    });

    state.animationId = requestAnimationFrame(nextTime => animate(container, nextTime));
}

function renderStatic(state) {
    state.frontViewContainer.style.visibility = "visible";
    state.backViewContainer.style.visibility = "hidden";
    state.layers.forEach(layer => {
        layer.element.removeAttribute("transform");
        state.frontViewContainer.appendChild(layer.element);
    });
}

function resolveRoot(root) {
    if (root instanceof Element) {
        return root;
    }

    return document.querySelector("[data-khb-logo-root]");
}

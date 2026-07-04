/* wwwroot/JS/Rotate.js */

// Settings
const ROTATION_SPEED = 0.0005;
const LAYER_SPACING = 0.5;
const LAYER_COUNT = 15;
const MIN_SCALE = 0.005;

// Variables
let animationId;
let frontViewContainer;
let backViewContainer;
let layers = [];

export function init3DLogo() {

    const spinningGroup = document.getElementById('spinning-group');
    const canvas = document.getElementById('canvas');

    if (!spinningGroup || spinningGroup.hasChildNodes()) return;

    fetch('Logo/logo.svg')
        .then(response => {
            if (!response.ok) throw new Error("HTTP error " + response.status);
            return response.text();
        })
        .then(svgText => {
            const parser = new DOMParser();
            const svgDoc = parser.parseFromString(svgText, "image/svg+xml");
            const svgEl = svgDoc.documentElement;

            // 1. GET VIEWBOX & CALCULATE CENTER
            let vbString = svgEl.getAttribute('viewBox');
            let viewBox = [0, 0, 100, 100]; // Default

            if (vbString) {
                // Parse numbers from string "0 0 500 500"
                viewBox = vbString.split(/[\s,]+/).map(parseFloat);
                canvas.setAttribute('viewBox', vbString);
            } else {
                // Fallback to width/height if viewbox missing
                const w = parseFloat(svgEl.getAttribute('width')) || 100;
                const h = parseFloat(svgEl.getAttribute('height')) || 100;
                viewBox = [0, 0, w, h];
                canvas.setAttribute('viewBox', `0 0 ${w} ${h}`);
            }

            // Calculate exact center coordinates (X + Width/2, Y + Height/2)
            const centerX = viewBox[0] + (viewBox[2] / 2);
            const centerY = viewBox[1] + (viewBox[3] / 2);

            // Format CSS string for center rotation
            const centerOriginCss = `${centerX}px ${centerY}px`;

            // 2. COPY STYLES & DEFS (Keep colors)
            const defs = svgEl.querySelectorAll('defs');
            const styles = svgEl.querySelectorAll('style');
            defs.forEach(def => canvas.prepend(def.cloneNode(true)));
            styles.forEach(style => canvas.prepend(style.cloneNode(true)));

            // 3. CREATE CONTAINERS
            frontViewContainer = document.createElementNS("http://www.w3.org/2000/svg", "g");
            backViewContainer = document.createElementNS("http://www.w3.org/2000/svg", "g");
            spinningGroup.appendChild(backViewContainer);
            spinningGroup.appendChild(frontViewContainer);

            // 4. GET VISUAL ELEMENTS
            const elementsToClone = Array.from(svgEl.children).filter(node =>
                node.nodeType === 1 &&
                !['defs', 'style', 'metadata'].includes(node.tagName.toLowerCase())
            );

            // 5. BUILD LAYERS
            elementsToClone.forEach((originalNode) => {
                for (let i = 0; i < LAYER_COUNT; i++) {
                    const clone = originalNode.cloneNode(true);

                    // FIX: Force Rotation from Center
                    clone.style.transformOrigin = centerOriginCss;
                    // Ensure transform box uses the viewbox coordinates
                    clone.style.transformBox = "view-box";

                    const zOffset = (i - LAYER_COUNT / 2) * LAYER_SPACING;

                    // Apply Depth Shading
                    if (i > 0 && i < LAYER_COUNT - 1) {
                        clone.style.filter = "brightness(0.85)";
                    }

                    const layerObj = {
                        element: clone,
                        z: zOffset,
                    };

                    layers.push(layerObj);
                    backViewContainer.appendChild(clone);
                }
            });

            requestAnimationFrame(animate);
        })
        .catch(err => console.error("Error loading SVG:", err));
}

function animate(time) {
    const angle = time * ROTATION_SPEED;
    const cosAngle = Math.cos(angle);
    const sinAngle = Math.sin(angle);

    const isFrontView = cosAngle > 0;

    if (isFrontView) {
        frontViewContainer.style.visibility = "visible";
        backViewContainer.style.visibility = "hidden";
    } else {
        frontViewContainer.style.visibility = "hidden";
        backViewContainer.style.visibility = "visible";
    }

    layers.forEach(layer => {
        const x = layer.z * sinAngle;
        let scaleX = Math.max(Math.abs(cosAngle), MIN_SCALE);
        const direction = isFrontView ? 1 : -1;

        // The transform-origin set in init3DLogo handles the center point.
        // We just apply the translation (depth) and scale (rotation).
        layer.element.setAttribute('transform',
            `translate(${x}, 0) scale(${scaleX * direction}, 1)`
        );

        if (isFrontView && layer.element.parentElement !== frontViewContainer) {
            frontViewContainer.appendChild(layer.element);
        } else if (!isFrontView && layer.element.parentElement !== backViewContainer) {
            backViewContainer.appendChild(layer.element);
        }
    });

    animationId = requestAnimationFrame(animate);
}

// main.js

// 1. CONFIGURATION
const ROTATION_SPEED = 0.0005;
const THICKNESS = 16;
const TOTAL_LAYERS = 20;
// Increased minimum scale to prevent "Gap" flashing
const MIN_SCALE = 0.01;

// 2. STATE
let currentAngle = 0;
let spinningGroup = document.getElementById("spinning-group");

// Create containers
const frontViewContainer = document.createElementNS(
  "http://www.w3.org/2000/svg",
  "g",
);
const backViewContainer = document.createElementNS(
  "http://www.w3.org/2000/svg",
  "g",
);

let frontLayers = [];
let backLayers = [];

// 3. INITIALIZATION
fetch("./Logo/logo.svg")
  .then((response) => response.text())
  .then((svgText) => {
    const parser = new DOMParser();
    const doc = parser.parseFromString(svgText, "image/svg+xml");

    const baseShape = doc.getElementById("base-shape");
    const whiteContent = doc.getElementById("white-content");

    if (!baseShape || !whiteContent) {
      console.error("Missing elements in SVG");
      return;
    }

    build3DObject(baseShape, whiteContent);

    spinningGroup.innerHTML = "";
    spinningGroup.appendChild(frontViewContainer);
    spinningGroup.appendChild(backViewContainer);

    // Initial State: Hide one, Show one using VISIBILITY (faster than display)
    backViewContainer.style.visibility = "hidden";

    requestAnimationFrame(animate);
  })
  .catch((err) => console.error("Error loading SVG", err));

function build3DObject(baseNode, contentNode) {
  // Measure Center
  spinningGroup.appendChild(baseNode);
  const box = baseNode.getBBox();
  spinningGroup.removeChild(baseNode);

  const cx = box.x + box.width / 2;
  const cy = box.y + box.height / 2;
  const centerTransform = `translate(${-cx}, ${-cy})`;

  let tempLayersData = [];

  for (let i = 0; i < TOTAL_LAYERS; i++) {
    const layerGroup = document.createElementNS(
      "http://www.w3.org/2000/svg",
      "g",
    );

    const baseClone = baseNode.cloneNode(true);
    baseClone.removeAttribute("id");
    baseClone.setAttribute("transform", centerTransform);

    // --- COLOR & STROKE FIX ---
    if (i > 0 && i < TOTAL_LAYERS - 1) {
      // Middle Layers (The Sides)
      baseClone.style.fill = "#009e4d";
      // Add stroke to fill gaps between layers during rotation
      baseClone.style.stroke = "#009e4d";
      baseClone.style.strokeWidth = "1px";
    } else {
      // Front and Back Faces
      baseClone.style.fill = "#00C35A";
    }

    layerGroup.appendChild(baseClone);

    // Add White Content (First and Last layer only)
    if (i === 0 || i === TOTAL_LAYERS - 1) {
      const contentClone = contentNode.cloneNode(true);
      contentClone.removeAttribute("id");
      contentClone.style.fill = "#FFFFFF";
      contentClone.setAttribute("transform", centerTransform);
      layerGroup.appendChild(contentClone);
    }

    const zPos = (i / (TOTAL_LAYERS - 1)) * THICKNESS - THICKNESS / 2;

    tempLayersData.push({ node: layerGroup, z: zPos });
  }

  // Front Container (Back-to-Front Order)
  tempLayersData.forEach((item) => {
    const clone = item.node.cloneNode(true);
    frontViewContainer.appendChild(clone);
    frontLayers.push({ element: clone, z: item.z });
  });

  // Back Container (Front-to-Back Order)
  for (let i = tempLayersData.length - 1; i >= 0; i--) {
    const item = tempLayersData[i];
    const clone = item.node.cloneNode(true);
    backViewContainer.appendChild(clone);
    backLayers.push({ element: clone, z: item.z });
  }
}

// 4. ANIMATION LOOP
function animate() {
  currentAngle += ROTATION_SPEED;

  const rawCos = Math.cos(currentAngle);
  const sinAngle = Math.sin(currentAngle);

  // --- CLAMP SCALE FIX ---
  let scaleVal = rawCos;
  if (Math.abs(scaleVal) < MIN_SCALE) {
    scaleVal = scaleVal >= 0 ? MIN_SCALE : -MIN_SCALE;
  }

  // --- VISIBILITY TOGGLE FIX ---
  // Use 'visibility' instead of 'display' to stop layout thrashing
  if (rawCos > 0) {
    if (frontViewContainer.style.visibility !== "visible") {
      frontViewContainer.style.visibility = "visible";
      backViewContainer.style.visibility = "hidden";
    }
    updateLayers(frontLayers, scaleVal, sinAngle);
  } else {
    if (backViewContainer.style.visibility !== "visible") {
      backViewContainer.style.visibility = "visible";
      frontViewContainer.style.visibility = "hidden";
    }
    updateLayers(backLayers, scaleVal, sinAngle);
  }

  requestAnimationFrame(animate);
}

function updateLayers(layersArray, scaleVal, sinAngle) {
  for (let i = 0; i < layersArray.length; i++) {
    const layer = layersArray[i];
    const xShift = layer.z * sinAngle;
    // Apply Transform
    layer.element.setAttribute(
      "transform",
      `translate(${xShift}, 0) scale(${scaleVal}, 1)`,
    );
  }
}

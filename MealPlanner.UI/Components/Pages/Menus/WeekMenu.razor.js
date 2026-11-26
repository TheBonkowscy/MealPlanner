let shiftHeld = false;

function onDragStart() {
    shiftHeld = false;
}

function onDragEvent(event) {
    shiftHeld = event.shiftKey;
}

export function start() {
    stop();
    document.addEventListener("dragstart", onDragStart, true);
    document.addEventListener("dragover", onDragEvent, true);
    document.addEventListener("drop", onDragEvent, true);
}

export function stop() {
    document.removeEventListener("dragstart", onDragStart, true);
    document.removeEventListener("dragover", onDragEvent, true);
    document.removeEventListener("drop", onDragEvent, true);
}

export function wasShiftHeld() {
    return shiftHeld;
}

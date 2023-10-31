function check(checkId, elementId) {
    const c = document.getElementById(checkId);
    document.getElementById(elementId).disabled = c.checked === false;
}
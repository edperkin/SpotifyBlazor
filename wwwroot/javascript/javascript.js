function check(checkId, elementId) {
    const c = document.getElementById(checkId);
    document.getElementById(elementId).disabled = c.checked === false;
}

function myFunction(label, checkId) {
    const checkBox = document.getElementById(checkId);
    label.style.backgroundColor = checkBox.checked ? '#8a1139' : '#000000';
}
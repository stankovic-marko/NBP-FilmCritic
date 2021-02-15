// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.getElementById("searchKeyword").addEventListener("keydown", keyDown);

function keyDown(event) {
    if (event.key === "Enter") {
        search();
    }
}

function search() {
    const keyword = document.getElementById("searchKeyword").value;
    window.location.href = `/search/?keyword=${keyword}`;
}
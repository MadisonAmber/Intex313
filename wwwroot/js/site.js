// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function initMap(longitude, latitude) {
    const location = { lat: latitude, lng: longitude };
    const map = new google.maps.Map(document.getElementById("map").{
        zoom: 4,
        center: location,
    });
    const marker = new google.maps.Marker({
        position: location,
        map: map,
    });
}

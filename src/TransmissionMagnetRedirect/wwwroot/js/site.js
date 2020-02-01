// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$("#registerHandlerButton").on("click", () =>
{
    var url = `${window.location.protocol}//${window.location.host}/Process/Magnet/%s`;
    navigator.registerProtocolHandler("magnet", url, "TransmissionMagnetRedirect");
});
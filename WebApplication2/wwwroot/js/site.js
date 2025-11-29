// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    $('#sidebar-toggler').on('click', function () {
        $('body').toggleClass('sidebar-collapsed');
    });

    // Initialize DataTables on any table with the 'datatable' class
    $('.datatable').DataTable();
});

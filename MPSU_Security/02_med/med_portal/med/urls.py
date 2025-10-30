from django.urls import path
from django.contrib.auth import views as auth_views
from . import views

app_name = "med"

urlpatterns = [
    path("crash/", views.crash, name="crash"),
    path("api/users/<int:user_id>/export/", views.export_user_profile, name="export_user_profile"),
    path("download/", views.download_by_token, name="download_by_token"),

    path("", views.index, name="index"),
    path("login/", auth_views.LoginView.as_view(template_name="med/login.html"), name="login"),
    path("logout/", auth_views.LogoutView.as_view(next_page="med:login"), name="logout"),

    # Doctor area (protected)
    path("doctor/patients/", views.clinic_patients_list, name="list"),
    path("doctor/patients/<int:patient_id>/", views.clinic_patient_detail, name="detail"),
    path("files/<int:report_id>/download/", views.download_report_protected, name="download"),

    # Admin
    path("ui/admin/dashboard/", views.admin_dashboard, name="admin_dashboard"),
]

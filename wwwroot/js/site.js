// Student Placement Management System - Interactive Logic

document.addEventListener("DOMContentLoaded", function () {
    // 1. Mobile Sidebar Toggle
    const sidebarToggle = document.getElementById("sidebarToggle");
    const appSidebar = document.querySelector(".app-sidebar");
    let backdrop = document.querySelector(".sidebar-backdrop");

    if (!backdrop) {
        backdrop = document.createElement("div");
        backdrop.className = "sidebar-backdrop";
        document.body.appendChild(backdrop);
    }

    if (sidebarToggle && appSidebar) {
        sidebarToggle.addEventListener("click", function () {
            appSidebar.classList.toggle("show");
            backdrop.classList.toggle("show");
        });

        backdrop.addEventListener("click", function () {
            appSidebar.classList.remove("show");
            backdrop.classList.remove("show");
        });
    }

    // 2. Fetch Unread Notifications Count
    function updateNotificationBadge() {
        fetch("/Notification/GetUnreadCount")
            .then(res => res.json())
            .then(data => {
                const badge = document.getElementById("unreadNotificationBadge");
                if (badge) {
                    if (data && data.count > 0) {
                        badge.textContent = data.count > 99 ? "99+" : data.count;
                        badge.style.display = "inline-block";
                    } else {
                        badge.style.display = "none";
                    }
                }
            })
            .catch(() => { /* silent fallback */ });
    }

    if (document.getElementById("unreadNotificationBadge")) {
        updateNotificationBadge();
        // Periodically refresh notification badge every 45 seconds
        setInterval(updateNotificationBadge, 45000);
    }

    // 3. Auto dismiss alert messages after 5 seconds
    const flashAlerts = document.querySelectorAll(".alert-dismissible");
    flashAlerts.forEach(function (alert) {
        setTimeout(function () {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            if (bsAlert) {
                bsAlert.close();
            }
        }, 6000);
    });
});

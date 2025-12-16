// Chart.js Configuration and Helpers for Dashboard
class DashboardCharts {
    constructor() {
        this.charts = {};
        this.colors = {
            primary: '#0066cc',
            success: '#28a745',
            warning: '#ffc107',
            danger: '#dc3545',
            info: '#17a2b8',
            secondary: '#6c757d',
            light: '#f8f9fa',
            dark: '#343a40'
        };
    }

    // Create Metrics Bar Chart
    createMetricsChart(containerId, data) {
        const ctx = document.getElementById(containerId);
        if (!ctx) return null;

        const config = {
            type: 'bar',
            data: {
                labels: ['Total Issues', 'My Issues', 'Available', 'Completion Rate (%)'],
                datasets: [{
                    label: 'Metrics',
                    data: [data.totalIssues, data.myIssues, data.available, data.completionRate],
                    backgroundColor: [
                        this.colors.primary,
                        this.colors.info,
                        this.colors.warning,
                        this.colors.success
                    ],
                    borderColor: [
                        this.colors.primary,
                        this.colors.info,
                        this.colors.warning,
                        this.colors.success
                    ],
                    borderWidth: 1,
                    borderRadius: 8,
                    borderSkipped: false
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: false, // Disable animations
                plugins: {
                    title: {
                        display: true,
                        text: 'Key Metrics Overview',
                        font: { size: 16, weight: 'bold' },
                        padding: { bottom: 20 }
                    },
                    legend: {
                        display: false
                    },
                    tooltip: {
                        backgroundColor: 'rgba(0, 0, 0, 0.8)',
                        titleColor: '#fff',
                        bodyColor: '#fff',
                        borderColor: '#ddd',
                        borderWidth: 1,
                        cornerRadius: 8,
                        displayColors: false
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: {
                            color: '#f1f1f1'
                        },
                        ticks: {
                            color: '#666'
                        }
                    },
                    x: {
                        grid: {
                            display: false
                        },
                        ticks: {
                            color: '#666'
                        }
                    }
                }
            }
        };

        this.charts[containerId] = new Chart(ctx, config);
        return this.charts[containerId];
    }

    // Create System Overview Chart for Admin Dashboard
    createSystemOverviewChart(containerId, data) {
        const ctx = document.getElementById(containerId);
        if (!ctx) return null;

        const config = {
            type: 'bar',
            data: {
                labels: ['Total Issues', 'Open Issues', 'In Progress', 'Resolved'],
                datasets: [{
                    label: 'System Metrics',
                    data: [data.totalIssues, data.openIssues, data.inProgressIssues, data.closedIssues],
                    backgroundColor: [
                        this.colors.primary,
                        this.colors.warning,
                        this.colors.info,
                        this.colors.success
                    ],
                    borderColor: [
                        this.colors.primary,
                        this.colors.warning,
                        this.colors.info,
                        this.colors.success
                    ],
                    borderWidth: 1,
                    borderRadius: 8,
                    borderSkipped: false
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: false, // Disable animations
                plugins: {
                    title: {
                        display: true,
                        text: 'System Issue Overview',
                        font: { size: 16, weight: 'bold' },
                        padding: { bottom: 20 }
                    },
                    legend: {
                        display: false
                    },
                    tooltip: {
                        backgroundColor: 'rgba(0, 0, 0, 0.8)',
                        titleColor: '#fff',
                        bodyColor: '#fff',
                        borderColor: '#ddd',
                        borderWidth: 1,
                        cornerRadius: 8,
                        displayColors: false
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: {
                            color: '#f1f1f1'
                        },
                        ticks: {
                            color: '#666'
                        }
                    },
                    x: {
                        grid: {
                            display: false
                        },
                        ticks: {
                            color: '#666'
                        }
                    }
                }
            }
        };

        this.charts[containerId] = new Chart(ctx, config);
        return this.charts[containerId];
    }

    // Create User Distribution Chart
    createUserDistributionChart(containerId, data) {
        const ctx = document.getElementById(containerId);
        if (!ctx) return null;

        const config = {
            type: 'doughnut',
            data: {
                labels: ['Customers', 'Engineers', 'Admins'],
                datasets: [{
                    data: [data.customers, data.engineers, data.admins],
                    backgroundColor: [
                        this.colors.primary,
                        this.colors.success,
                        this.colors.danger
                    ],
                    borderColor: '#fff',
                    borderWidth: 3,
                    hoverBorderWidth: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: false, // Disable animations
                plugins: {
                    title: {
                        display: true,
                        text: 'User Distribution',
                        font: { size: 16, weight: 'bold' },
                        padding: { bottom: 20 }
                    },
                    legend: {
                        position: 'bottom',
                        labels: {
                            padding: 20,
                            usePointStyle: true,
                            font: { size: 12 }
                        }
                    },
                    tooltip: {
                        backgroundColor: 'rgba(0, 0, 0, 0.8)',
                        titleColor: '#fff',
                        bodyColor: '#fff',
                        borderColor: '#ddd',
                        borderWidth: 1,
                        cornerRadius: 8,
                        callbacks: {
                            label: function(context) {
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const percentage = total > 0 ? Math.round((context.raw / total) * 100) : 0;
                                return `${context.label}: ${context.raw} (${percentage}%)`;
                            }
                        }
                    }
                },
                cutout: '60%'
            }
        };

        this.charts[containerId] = new Chart(ctx, config);
        return this.charts[containerId];
    }

    // Create Status Breakdown Chart
    createStatusBreakdownChart(containerId, data) {
        const ctx = document.getElementById(containerId);
        if (!ctx) return null;

        const config = {
            type: 'pie',
            data: {
                labels: ['Open', 'In Progress', 'Resolved'],
                datasets: [{
                    data: [data.open, data.inProgress, data.resolved],
                    backgroundColor: [
                        this.colors.warning,
                        this.colors.info,
                        this.colors.success
                    ],
                    borderColor: '#fff',
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: false, // Disable animations
                plugins: {
                    title: {
                        display: true,
                        text: 'Issue Status Distribution',
                        font: { size: 16, weight: 'bold' },
                        padding: { bottom: 20 }
                    },
                    legend: {
                        position: 'bottom',
                        labels: {
                            padding: 15,
                            usePointStyle: true,
                            font: { size: 12 }
                        }
                    },
                    tooltip: {
                        backgroundColor: 'rgba(0, 0, 0, 0.8)',
                        titleColor: '#fff',
                        bodyColor: '#fff',
                        borderColor: '#ddd',
                        borderWidth: 1,
                        cornerRadius: 8,
                        callbacks: {
                            label: function(context) {
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const percentage = total > 0 ? Math.round((context.raw / total) * 100) : 0;
                                return `${context.label}: ${context.raw} (${percentage}%)`;
                            }
                        }
                    }
                }
            }
        };

        this.charts[containerId] = new Chart(ctx, config);
        return this.charts[containerId];
    }

    // Create Priority Donut Chart
    createPriorityChart(containerId, data) {
        const ctx = document.getElementById(containerId);
        if (!ctx) return null;

        const config = {
            type: 'doughnut',
            data: {
                labels: ['High Priority', 'Medium Priority', 'Low Priority'],
                datasets: [{
                    data: [data.high, data.medium, data.low],
                    backgroundColor: [
                        this.colors.danger,
                        this.colors.warning,
                        this.colors.success
                    ],
                    borderColor: '#fff',
                    borderWidth: 3,
                    hoverBorderWidth: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: false, // Disable animations
                plugins: {
                    title: {
                        display: true,
                        text: 'Issues by Priority',
                        font: { size: 16, weight: 'bold' },
                        padding: { bottom: 20 }
                    },
                    legend: {
                        position: 'bottom',
                        labels: {
                            padding: 20,
                            usePointStyle: true,
                            font: { size: 12 }
                        }
                    },
                    tooltip: {
                        backgroundColor: 'rgba(0, 0, 0, 0.8)',
                        titleColor: '#fff',
                        bodyColor: '#fff',
                        borderColor: '#ddd',
                        borderWidth: 1,
                        cornerRadius: 8,
                        callbacks: {
                            label: function(context) {
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const percentage = total > 0 ? Math.round((context.raw / total) * 100) : 0;
                                return `${context.label}: ${context.raw} (${percentage}%)`;
                            }
                        }
                    }
                },
                cutout: '60%'
            }
        };

        this.charts[containerId] = new Chart(ctx, config);
        return this.charts[containerId];
    }

    // Create Status Distribution Chart
    createStatusChart(containerId, data) {
        const ctx = document.getElementById(containerId);
        if (!ctx) return null;

        const config = {
            type: 'bar',
            data: {
                labels: ['Open', 'In Progress', 'Resolved', 'Overdue'],
                datasets: [{
                    label: 'Issues by Status',
                    data: [data.open, data.inProgress, data.resolved, data.overdue],
                    backgroundColor: [
                        this.colors.info,
                        this.colors.warning,
                        this.colors.success,
                        this.colors.danger
                    ],
                    borderColor: [
                        this.colors.info,
                        this.colors.warning,
                        this.colors.success,
                        this.colors.danger
                    ],
                    borderWidth: 1,
                    borderRadius: 8,
                    borderSkipped: false
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: false, // Disable animations
                indexAxis: 'y',
                plugins: {
                    title: {
                        display: true,
                        text: 'Issues by Status',
                        font: { size: 16, weight: 'bold' },
                        padding: { bottom: 20 }
                    },
                    legend: {
                        display: false
                    },
                    tooltip: {
                        backgroundColor: 'rgba(0, 0, 0, 0.8)',
                        titleColor: '#fff',
                        bodyColor: '#fff',
                        borderColor: '#ddd',
                        borderWidth: 1,
                        cornerRadius: 8,
                        displayColors: false
                    }
                },
                scales: {
                    x: {
                        beginAtZero: true,
                        grid: {
                            color: '#f1f1f1'
                        },
                        ticks: {
                            color: '#666'
                        }
                    },
                    y: {
                        grid: {
                            display: false
                        },
                        ticks: {
                            color: '#666'
                        }
                    }
                }
            }
        };

        this.charts[containerId] = new Chart(ctx, config);
        return this.charts[containerId];
    }

    // Create Workload Trend Chart
    createTrendChart(containerId, data) {
        const ctx = document.getElementById(containerId);
        if (!ctx) return null;

        const config = {
            type: 'line',
            data: {
                labels: data.labels || [],
                datasets: [{
                    label: 'Issues Created',
                    data: data.created || [],
                    borderColor: this.colors.primary,
                    backgroundColor: this.colors.primary + '20',
                    fill: true,
                    tension: 0.4,
                    pointBackgroundColor: this.colors.primary,
                    pointBorderColor: '#fff',
                    pointBorderWidth: 2,
                    pointRadius: 5
                }, {
                    label: 'Issues Resolved',
                    data: data.resolved || [],
                    borderColor: this.colors.success,
                    backgroundColor: this.colors.success + '20',
                    fill: true,
                    tension: 0.4,
                    pointBackgroundColor: this.colors.success,
                    pointBorderColor: '#fff',
                    pointBorderWidth: 2,
                    pointRadius: 5
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: false, // Disable animations
                plugins: {
                    title: {
                        display: true,
                        text: 'Issue Trends (Last 7 Days)',
                        font: { size: 16, weight: 'bold' },
                        padding: { bottom: 20 }
                    },
                    legend: {
                        position: 'bottom',
                        labels: {
                            padding: 20,
                            usePointStyle: true,
                            font: { size: 12 }
                        }
                    },
                    tooltip: {
                        backgroundColor: 'rgba(0, 0, 0, 0.8)',
                        titleColor: '#fff',
                        bodyColor: '#fff',
                        borderColor: '#ddd',
                        borderWidth: 1,
                        cornerRadius: 8,
                        mode: 'index',
                        intersect: false
                    }
                },
                scales: {
                    x: {
                        grid: {
                            color: '#f1f1f1'
                        },
                        ticks: {
                            color: '#666'
                        }
                    },
                    y: {
                        beginAtZero: true,
                        grid: {
                            color: '#f1f1f1'
                        },
                        ticks: {
                            color: '#666'
                        }
                    }
                },
                interaction: {
                    mode: 'index',
                    intersect: false
                }
            }
        };

        this.charts[containerId] = new Chart(ctx, config);
        return this.charts[containerId];
    }

    // Create Performance Gauge Chart
    createGaugeChart(containerId, value) {
        const ctx = document.getElementById(containerId);
        if (!ctx) return null;

        const config = {
            type: 'doughnut',
            data: {
                datasets: [{
                    data: [value, 100 - value],
                    backgroundColor: [
                        value >= 80 ? this.colors.success : 
                        value >= 60 ? this.colors.warning : this.colors.danger,
                        '#e9ecef'
                    ],
                    borderWidth: 0,
                    cutout: '80%'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: false, // Disable animations
                plugins: {
                    title: {
                        display: true,
                        text: 'Completion Rate',
                        font: { size: 16, weight: 'bold' },
                        padding: { bottom: 20 }
                    },
                    legend: {
                        display: false
                    },
                    tooltip: {
                        enabled: false
                    }
                }
            },
            plugins: [{
                id: 'gaugeText',
                beforeDraw: function(chart) {
                    const ctx = chart.ctx;
                    const centerX = chart.getDatasetMeta(0).data[0].x;
                    const centerY = chart.getDatasetMeta(0).data[0].y;
                    
                    ctx.save();
                    ctx.textAlign = 'center';
                    ctx.textBaseline = 'middle';
                    ctx.font = 'bold 24px sans-serif';
                    ctx.fillStyle = '#333';
                    ctx.fillText(value + '%', centerX, centerY);
                    ctx.restore();
                }
            }]
        };

        this.charts[containerId] = new Chart(ctx, config);
        return this.charts[containerId];
    }

    // Destroy all charts
    destroyAllCharts() {
        Object.values(this.charts).forEach(chart => {
            if (chart) chart.destroy();
        });
        this.charts = {};
    }

    // Destroy specific chart
    destroyChart(containerId) {
        if (this.charts[containerId]) {
            this.charts[containerId].destroy();
            delete this.charts[containerId];
        }
    }
}

// Global instance
window.DashboardCharts = DashboardCharts;
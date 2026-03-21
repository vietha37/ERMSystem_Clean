"use client";

import React, { useState, useEffect } from "react";
import { Card } from "@/components/ui/Card";
import { dashboardService } from "@/services/dashboardService";
import toast from "react-hot-toast";

export default function DashboardPage() {
  const [data, setData] = useState<any>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchStats = async () => {
      try {
        const result = await dashboardService.getStats();
        setData(result);
      } catch (error) {
        toast.error("Failed to load dashboard statistics.");
        // Fallback mock data if API fails
        setData({
          totalPatients: 0,
          appointmentsToday: 0,
          completedAppointments: 0,
          topDiagnoses: [
             { name: "Mock Hypertension", count: 12 },
             { name: "Mock Diabetes", count: 8 }
          ]
        });
      } finally {
        setIsLoading(false);
      }
    };
    fetchStats();
  }, []);

  if (isLoading) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[60vh]">
        <div className="w-12 h-12 border-4 border-blue-200 border-t-blue-600 rounded-full animate-spin"></div>
        <p className="mt-4 text-gray-500 font-medium tracking-wide">Loading statistics...</p>
      </div>
    );
  }

  const stats = [
    { label: "Total Patients", value: data?.totalPatients || 0, icon: "👥" },
    { label: "Appointments Today", value: data?.appointmentsToday || 0, icon: "📅" },
    { label: "Completed Appointments", value: data?.completedAppointments || 0, icon: "✅" },
  ];

  return (
    <div className="space-y-6 animate-fade-in">
      <div className="flex justify-between items-center mb-4">
        <h1 className="text-2xl font-bold text-gray-800">Dashboard Overview</h1>
        <button className="bg-blue-600 text-white px-4 py-2 rounded-xl font-medium shadow-sm hover:shadow hover:bg-blue-500 transition-all duration-200">
          + New Appointment
        </button>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {stats.map((stat, i) => (
          <Card key={i} className="bg-gradient-to-br from-blue-500 to-blue-600 border-none shadow-md hover:shadow-lg transform hover:-translate-y-1 transition-all duration-300">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-blue-100 text-sm font-medium mb-1">{stat.label}</p>
                <h3 className="text-4xl font-bold text-white">{stat.value}</h3>
              </div>
              <div className="text-5xl opacity-80">{stat.icon}</div>
            </div>
          </Card>
        ))}
      </div>

      {/* Main Content Area */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-6">
          <Card className="h-full min-h-[400px]">
            <div className="flex justify-between items-center border-b border-gray-100 pb-4 mb-4">
               <h2 className="text-xl font-semibold text-gray-800">Today's Schedule</h2>
               <span className="bg-blue-50 text-blue-600 text-sm font-medium px-3 py-1 rounded-full">{data?.appointmentsToday || 0} total</span>
            </div>
            
            <div className="flex flex-col items-center justify-center h-64 text-gray-400">
              <span className="text-4xl mb-3">🕒</span>
              <p>Schedule list will load here...</p>
            </div>
          </Card>
        </div>
        
        <div className="space-y-6">
          <Card>
            <h2 className="text-xl font-semibold text-gray-800 border-b border-gray-100 pb-4 mb-4">Top 5 Diagnoses</h2>
            <div className="space-y-4">
              {data?.topDiagnoses?.length > 0 ? data.topDiagnoses.map((diag: any, index: number) => (
                <div key={index} className="flex items-center justify-between p-3 rounded-lg hover:bg-blue-50 transition-colors">
                  <div className="flex items-center gap-3">
                    <div className="w-8 h-8 rounded-full bg-blue-100 text-blue-600 flex items-center justify-center font-bold text-sm">
                      {index + 1}
                    </div>
                    <span className="font-medium text-gray-700">{diag.name || diag.Name}</span>
                  </div>
                  <span className="bg-blue-50 text-blue-700 px-3 py-1 rounded-full text-sm font-semibold">
                    {diag.count || diag.Count}
                  </span>
                </div>
              )) : (
                <p className="text-sm text-gray-500 text-center py-4">No top diagnoses found.</p>
              )}
            </div>
          </Card>
        </div>
      </div>

    </div>
  );
}

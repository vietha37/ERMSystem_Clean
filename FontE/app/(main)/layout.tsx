import { Sidebar } from "@/components/layout/Sidebar";
import { Header } from "@/components/layout/Header";
import ProtectedLayout from "@/components/layout/ProtectedLayout";

export default function MainLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <ProtectedLayout>
      <div className="flex min-h-screen bg-blue-50/30">
        <Sidebar />
        <div className="flex-1 ml-64 flex flex-col pt-20">
          <Header />
          <main className="flex-1 p-6 overflow-y-auto scrollbar-thin scrollbar-thumb-gray-200">
            {children}
          </main>
        </div>
      </div>
    </ProtectedLayout>
  );
}

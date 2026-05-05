import type { Metadata } from "next";
import { Cormorant_Garamond, Manrope } from "next/font/google";
import "./globals.css";

const bodyFont = Manrope({
  variable: "--font-manrope",
  subsets: ["latin"],
});

const displayFont = Cormorant_Garamond({
  variable: "--font-cormorant",
  subsets: ["latin"],
  weight: ["500", "600", "700"],
});

export const metadata: Metadata = {
  title: "ERM Private Hospital",
  description: "Nền tảng bệnh viện tư với khám đa khoa, xét nghiệm, chẩn đoán và chăm sóc gia đình.",
};

import { Toaster } from "react-hot-toast";

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="vi">
      <body className={`${bodyFont.variable} ${displayFont.variable} antialiased bg-[var(--background)] text-[var(--foreground)]`}>
        {children}
        <Toaster position="top-right" />
      </body>
    </html>
  );
}

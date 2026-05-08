import type { Metadata } from "next";
import { Cormorant_Garamond, Manrope } from "next/font/google";
import { Toaster } from "react-hot-toast";
import "./globals.css";

const bodyFont = Manrope({
  variable: "--font-manrope",
  subsets: ["latin", "latin-ext"],
});

const displayFont = Cormorant_Garamond({
  variable: "--font-cormorant",
  subsets: ["latin", "latin-ext"],
  weight: ["500", "600", "700"],
});

export const metadata: Metadata = {
  title: "ERM Private Hospital",
  description: "Nền tảng bệnh viện tư với khám đa khoa, xét nghiệm, chẩn đoán và chăm sóc gia đình.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="vi">
      <body className={`${bodyFont.variable} ${displayFont.variable} bg-[var(--background)] text-[var(--foreground)] antialiased`}>
        {children}
        <Toaster position="top-right" />
      </body>
    </html>
  );
}

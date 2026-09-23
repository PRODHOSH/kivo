import type { Metadata } from 'next';
import './globals.css';

export const metadata: Metadata = {
  metadataBase: new URL('https://kivo.prodhosh.me'),
  title: 'Kivo — tell your computer simply',
  description: 'Tell your computer simply. Kivo automates the boring stuff so you can do the creative stuff.',
  keywords: ['Kivo', 'SaaS', 'Automation', 'Productivity', 'Open Source'],
  openGraph: {
    title: 'Kivo — tell your computer simply',
    description: 'Tell your computer simply. Kivo automates the boring stuff.',
    url: 'https://kivo.prodhosh.me',
    siteName: 'Kivo',
    locale: 'en_US',
    type: 'website',
  },
  twitter: {
    card: 'summary_large_image',
    title: 'Kivo — tell your computer simply',
    description: 'Tell your computer simply. Kivo automates the boring stuff.',
  },
  icons: { icon: '/spirit.png' },
};

export default function RootLayout({children}: Readonly<{children: React.ReactNode}>) {
  return <html lang="en"><body>{children}</body></html>;
}

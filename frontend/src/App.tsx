import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { WagmiConfig } from 'wagmi';
import { RainbowKitProvider } from '@rainbow-me/rainbowkit';
import { Toaster } from 'react-hot-toast';

import { wagmiConfig, chains } from './config/wagmi';
import Header from './components/layout/Header';
import Footer from './components/layout/Footer';
import Home from './pages/Home';
import Properties from './pages/Properties';
import PropertyDetail from './pages/PropertyDetail';
import Portfolio from './pages/Portfolio';
import Staking from './pages/Staking';

import '@rainbow-me/rainbowkit/styles.css';
import './index.css';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 3,
      refetchOnWindowFocus: false,
    },
  },
});

function App() {
  return (
    <WagmiConfig config={wagmiConfig}>
      <RainbowKitProvider chains={chains}>
        <QueryClientProvider client={queryClient}>
          <Router>
            <div className="min-h-screen bg-gray-50 flex flex-col">
              <Header />
              <main className="flex-1">
                <Routes>
                  <Route path="/" element={<Home />} />
                  <Route path="/properties" element={<Properties />} />
                  <Route path="/properties/:id" element={<PropertyDetail />} />
                  <Route path="/portfolio" element={<Portfolio />} />
                  <Route path="/staking" element={<Staking />} />
                </Routes>
              </main>
              <Footer />
            </div>
            <Toaster 
              position="top-right"
              toastOptions={{
                duration: 4000,
                style: {
                  background: '#363636',
                  color: '#fff',
                },
              }}
            />
          </Router>
        </QueryClientProvider>
      </RainbowKitProvider>
    </WagmiConfig>
  );
}

export default App;

/** @type {import('next').NextConfig} */
const nextConfig = {
    async redirects() {
        return [
            {
                source: '/',
                destination: '/Login',
                permanent: false,
            },
        ];
    },
};

export default nextConfig;

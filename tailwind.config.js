/** @type {import('tailwindcss').Config} */
module.exports = {
    content: ["./src/**/*.{js,jsx,ts,tsx}"],
    theme: {
      extend: {
        colors: {  // Custom colors for Singular Systems (shades of blue and white)
          primaryBlue: '#1e40af', // A primary blue shade
          secondaryBlue: '#3b82f6', // A lighter blue
          lightBlue: '#60a5fa', // Light blue for softer background or accents
          white: '#ffffff', // White background or text color
          grayLight: '#f3f4f6', // A very light gray for backgrounds or borders

       
       },
      },
    },
    plugins: [],
  }
  
import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import App from "./App";
import keycloak from "./keycloak";

keycloak
  .init({
    onLoad: "login-required",
    pkceMethod: "S256",
  })
  .then((authenticated) => {
    if (!authenticated) {
      console.log("Not authenticated");
      return;
    }

    console.log("Authenticated!");
    console.log("Access Token:", keycloak.token);
    console.log("Parsed Token:", keycloak.tokenParsed);

    createRoot(document.getElementById("root")).render(
      <StrictMode>
        <App />
      </StrictMode>,
    );
  })
  .catch((error) => {
    console.error("Keycloak initialization failed:", error);
  });

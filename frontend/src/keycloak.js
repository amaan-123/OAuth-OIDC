import Keycloak from "keycloak-js";

const keycloak = new Keycloak({
  url: "http://localhost:8080",
  realm: "fullstack-lab",
  clientId: "react-spa",
});

export default keycloak;

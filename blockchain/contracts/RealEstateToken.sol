// SPDX-License-Identifier: MIT
pragma solidity ^0.8.19;

import "@openzeppelin/contracts/token/ERC721/ERC721.sol";
import "@openzeppelin/contracts/token/ERC721/extensions/ERC721Enumerable.sol";
import "@openzeppelin/contracts/token/ERC721/extensions/ERC721URIStorage.sol";
import "@openzeppelin/contracts/security/Pausable.sol";
import "@openzeppelin/contracts/access/Ownable.sol";
import "@openzeppelin/contracts/security/ReentrancyGuard.sol";
import "@openzeppelin/contracts/utils/Counters.sol";

/**
 * @title RealEstateToken
 * @dev NFT contract for tokenizing real estate properties
 */
contract RealEstateToken is 
    ERC721, 
    ERC721Enumerable, 
    ERC721URIStorage, 
    Pausable, 
    Ownable, 
    ReentrancyGuard 
{
    using Counters for Counters.Counter;

    Counters.Counter private _tokenIdCounter;

    struct Property {
        uint256 tokenId;
        string propertyAddress;
        uint256 totalValue;      // 총 부동산 가치 (wei)
        uint256 totalShares;     // 총 지분 수
        uint256 availableShares; // 판매 가능한 지분 수
        uint256 pricePerShare;   // 지분당 가격 (wei)
        address propertyOwner;   // 부동산 소유자
        bool isActive;          // 거래 활성화 여부
        uint256 createdAt;      // 생성 시간
        string metadataURI;     // 메타데이터 URI
    }

    struct ShareOwnership {
        uint256 tokenId;
        address owner;
        uint256 shares;
        uint256 purchasePrice;
        uint256 purchaseDate;
    }

    // 매핑
    mapping(uint256 => Property) public properties;
    mapping(uint256 => mapping(address => uint256)) public shareOwnership; // tokenId => owner => shares
    mapping(address => uint256[]) public userOwnedTokens;

    // 이벤트
    event PropertyTokenized(
        uint256 indexed tokenId, 
        string propertyAddress, 
        uint256 totalValue, 
        uint256 totalShares,
        address propertyOwner
    );

    event SharesPurchased(
        uint256 indexed tokenId, 
        address indexed buyer, 
        uint256 shares, 
        uint256 totalPrice
    );

    event SharesSold(
        uint256 indexed tokenId, 
        address indexed seller, 
        uint256 shares, 
        uint256 totalPrice
    );

    constructor() ERC721("RealEstateToken", "RET") {}

    /**
     * @dev 부동산 토큰화
     */
    function tokenizeProperty(
        string memory propertyAddress,
        uint256 totalValue,
        uint256 totalShares,
        uint256 availableShares,
        string memory metadataURI
    ) public whenNotPaused returns (uint256) {
        require(totalValue > 0, "Total value must be greater than 0");
        require(totalShares > 0, "Total shares must be greater than 0");
        require(availableShares <= totalShares, "Available shares cannot exceed total shares");

        uint256 tokenId = _tokenIdCounter.current();
        _tokenIdCounter.increment();

        // NFT 민팅
        _safeMint(msg.sender, tokenId);
        _setTokenURI(tokenId, metadataURI);

        // 부동산 정보 저장
        properties[tokenId] = Property({
            tokenId: tokenId,
            propertyAddress: propertyAddress,
            totalValue: totalValue,
            totalShares: totalShares,
            availableShares: availableShares,
            pricePerShare: totalValue / totalShares,
            propertyOwner: msg.sender,
            isActive: true,
            createdAt: block.timestamp,
            metadataURI: metadataURI
        });

        userOwnedTokens[msg.sender].push(tokenId);

        emit PropertyTokenized(tokenId, propertyAddress, totalValue, totalShares, msg.sender);

        return tokenId;
    }

    /**
     * @dev 지분 구매
     */
    function purchaseShares(uint256 tokenId, uint256 shares) 
        public 
        payable 
        whenNotPaused 
        nonReentrant 
    {
        Property storage property = properties[tokenId];
        require(property.isActive, "Property is not active for trading");
        require(shares > 0, "Shares must be greater than 0");
        require(shares <= property.availableShares, "Not enough shares available");

        uint256 totalPrice = property.pricePerShare * shares;
        require(msg.value >= totalPrice, "Insufficient payment");

        // 지분 소유권 업데이트
        shareOwnership[tokenId][msg.sender] += shares;
        property.availableShares -= shares;

        // 소유자에게 대금 전송
        payable(property.propertyOwner).transfer(totalPrice);

        // 초과 지불금 반환
        if (msg.value > totalPrice) {
            payable(msg.sender).transfer(msg.value - totalPrice);
        }

        emit SharesPurchased(tokenId, msg.sender, shares, totalPrice);
    }

    /**
     * @dev 지분 판매
     */
    function sellShares(uint256 tokenId, uint256 shares, uint256 pricePerShare) 
        public 
        whenNotPaused 
        nonReentrant 
    {
        require(shareOwnership[tokenId][msg.sender] >= shares, "Insufficient shares");
        require(shares > 0, "Shares must be greater than 0");

        Property storage property = properties[tokenId];
        require(property.isActive, "Property is not active for trading");

        // 지분 소유권 업데이트 (실제 구매자가 있을 때까지 대기 상태로 설정)
        shareOwnership[tokenId][msg.sender] -= shares;
        property.availableShares += shares;
        property.pricePerShare = pricePerShare;

        emit SharesSold(tokenId, msg.sender, shares, pricePerShare * shares);
    }

    /**
     * @dev 사용자의 지분 조회
     */
    function getShareOwnership(uint256 tokenId, address owner) 
        public 
        view 
        returns (uint256) 
    {
        return shareOwnership[tokenId][owner];
    }

    /**
     * @dev 부동산 정보 조회
     */
    function getProperty(uint256 tokenId) 
        public 
        view 
        returns (Property memory) 
    {
        return properties[tokenId];
    }

    /**
     * @dev 사용자 소유 토큰 목록 조회
     */
    function getUserOwnedTokens(address user) 
        public 
        view 
        returns (uint256[] memory) 
    {
        return userOwnedTokens[user];
    }

    // 관리자 함수들
    function pause() public onlyOwner {
        _pause();
    }

    function unpause() public onlyOwner {
        _unpause();
    }

    function setPropertyActive(uint256 tokenId, bool isActive) 
        public 
        onlyOwner 
    {
        properties[tokenId].isActive = isActive;
    }

    // 오버라이드 함수들
    function _beforeTokenTransfer(address from, address to, uint256 tokenId, uint256 batchSize)
        internal
        whenNotPaused
        override(ERC721, ERC721Enumerable)
    {
        super._beforeTokenTransfer(from, to, tokenId, batchSize);
    }

    function _burn(uint256 tokenId) internal override(ERC721, ERC721URIStorage) {
        super._burn(tokenId);
    }

    function tokenURI(uint256 tokenId)
        public
        view
        override(ERC721, ERC721URIStorage)
        returns (string memory)
    {
        return super.tokenURI(tokenId);
    }

    function supportsInterface(bytes4 interfaceId)
        public
        view
        override(ERC721, ERC721Enumerable)
        returns (bool)
    {
        return super.supportsInterface(interfaceId);
    }
}
